using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextPlayerDataLayer.Model;
using Windows.Storage;
using TagLib;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Diagnostics;
using NextPlayerDataLayer.Helpers;
using NextPlayerDataLayer.Constants;

namespace NextPlayerDataLayer.Services
{
    public delegate void MediaImportedHandler(string s); 

    public sealed class MediaImport
    {
        public static event MediaImportedHandler MediaImported;
        
        public static void OnMediaImported(string s)
        {
            if (MediaImported != null)
            {
                MediaImported(s);
            }
        }

        public async static Task ImportAndUpdateDatabase(IProgress<int> progress)
        {
            string dbVersion = Windows.Storage.ApplicationData.Current.LocalSettings.Values[AppConstants.DBVersion].ToString();
            if (dbVersion.EndsWith("notupdated"))
            {
                UpdateComposers(progress);
                Windows.Storage.ApplicationData.Current.LocalSettings.Values[AppConstants.DBVersion] = dbVersion.Replace("notupdated", "");
                return;
            }
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.MediaScan, true);
            IReadOnlyList<StorageFile> list = await KnownFolders.MusicLibrary.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);
            
            int count = 1;
            Tuple<int, int> tuple;
            Dictionary<string, Tuple<int, int>> dict = DatabaseManager.GetFilePaths();

            List<int> toAvailable = new List<int>();//lista songId
            List<SongData> newSongs = new List<SongData>();

            foreach (var file in list)
            {
                //Windows.Storage.FileProperties.BasicProperties bp = await file.GetBasicPropertiesAsync();
                // Sprawdzanie rozmiaru nie działa
                if (dict.TryGetValue(file.Path, out tuple))
                {
                    if (tuple.Item1 == 0)//zaznaczony jako niedostepny
                    {
                        toAvailable.Add(tuple.Item2);
                    }
                    else//zaznaczony jako dostepny
                    {
                        toAvailable.Add(tuple.Item2);
                    }
                }
                else
                {
                    SongData song = await CreateSongFromFile(file);
                    newSongs.Add(song);
                }
                
                //if (i == 0)
                //{
                //    //istnieje taka sama
                //}
                //else if (i > 0)
                //{
                //    SongData song = await CreateSongFromFile(file);
                //    await DatabaseManager.UpdateSongData(song, i);
                //}
                //else { };

                if (count % 10 == 0)//!!!
                {
                    progress.Report(count);
                }
                count++;
            }

            DatabaseManager.ChangeAvailability(toAvailable);
            Library.Current.CheckNPAfterUpdate(toAvailable);
            await DatabaseManager.InsertSongsAsync(newSongs);

            ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.MediaScan);
            OnMediaImported("Update");
            SendToast();
        }

        
        private async static Task<SongData> CreateSongFromFile(StorageFile file)
        {
            //var w1 = Stopwatch.StartNew();

            //Windows.Storage.FileProperties.MusicProperties mp = await file.Properties.GetMusicPropertiesAsync();
            Windows.Storage.FileProperties.BasicProperties bp = await file.GetBasicPropertiesAsync();
           
            SongData song = new SongData();

            song.DateAdded = DateTime.Now;
            song.Filename = file.Name;
            song.FileSize = bp.Size;
            song.Path = file.Path;
            song.PlayCount = 0;
            song.LastPlayed = DateTime.MinValue;
            song.IsAvailable = 1;

            //song.Album = mp.Album;
            //song.AlbumArtist = mp.AlbumArtist;
            //song.Artist = mp.Artist;
            //song.Bitrate = mp.Bitrate;
            
            //song.Duration = TimeSpan.FromMilliseconds(mp.Duration.Ticks);

            //song.Genre = mp.Genre.FirstOrDefault();
            //song.Lyrics = ""; //jak odczytac?
            //song.Title = mp.Title;
            //song.TrackNumber = mp.TrackNumber;
            //song.Year = mp.Year;

            song.Publisher = "";
            song.Rating = 0;
            song.Subtitle = "";
            
            if (Path.GetExtension(file.Path).ToLower().Equals(".aac"))
            {

            }
            Stream fileStream = await file.OpenStreamForReadAsync();
           
            try
            {
                var tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
                Tag tags;
                if (tagFile.TagTypes.ToString().Contains(TagTypes.Id3v2.ToString()))
                {
                    tags = tagFile.GetTag(TagTypes.Id3v2);
                }
                else if (tagFile.TagTypes.ToString().Contains(TagTypes.Id3v1.ToString()))
                {
                    tags = tagFile.GetTag(TagTypes.Id3v1);
                }
                else if (tagFile.TagTypes.ToString().Contains(TagTypes.Apple.ToString()))
                {
                    tags = tagFile.GetTag(TagTypes.Apple);
                }
                else
                {
                    tags = tagFile.GetTag(tagFile.TagTypes);
                }
                song.Album = tags.Album ?? "Unknown";
                song.AlbumArtist = tags.FirstAlbumArtist ?? "";
                song.Artist = tags.FirstPerformer ?? "Unknown";
                song.Bitrate = (uint)tagFile.Properties.AudioBitrate;
                song.Composer = tags.FirstComposer ?? "";
                song.Performer = tags.FirstPerformer ?? "";
                song.Duration = TimeSpan.FromSeconds(Convert.ToInt32(tagFile.Properties.Duration.TotalSeconds));
                song.Genre = tags.FirstGenre ?? "Unknown";
                song.Lyrics = tags.Lyrics ?? "";
                song.Title = tags.Title ?? file.DisplayName;
                song.TrackNumber = tags.Track;
                song.Year = tags.Year;
            }
            catch (CorruptFileException e)
            {
                song.Album = "Unknown";
                song.AlbumArtist = "";
                song.Artist = "Unknown";
                song.Bitrate = 0;
                song.Composer = "";
                song.Duration = TimeSpan.Zero;
                song.Genre = "Unknown";
                song.Lyrics = "";
                song.Performer = "";
                song.Title = file.DisplayName;
                song.TrackNumber = 0;
                song.Year = 0;
            }
          
            return song;
        }

        // Update composer and performer field
        public static async void UpdateComposers(IProgress<int> progress)
        {
            var list = DatabaseManager.GetSongItems();
            List<Tuple<int, string,string>> newTags = new List<Tuple<int, string, string>>();
            int count = 0;
            foreach (var song in list)
            {
                try
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
                    Stream fileStream = await file.OpenStreamForReadAsync();
                    try
                    {
                        var tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
                        Tag tags;
                        if (tagFile.TagTypes.ToString().Contains(TagTypes.Id3v2.ToString()))
                        {
                            tags = tagFile.GetTag(TagTypes.Id3v2);
                        }
                        else if (tagFile.TagTypes.ToString().Contains(TagTypes.Id3v1.ToString()))
                        {
                            tags = tagFile.GetTag(TagTypes.Id3v1);
                        }
                        else if (tagFile.TagTypes.ToString().Contains(TagTypes.Apple.ToString()))
                        {
                            tags = tagFile.GetTag(TagTypes.Apple);
                        }
                        else
                        {
                            tags = tagFile.GetTag(tagFile.TagTypes);
                        }
                        newTags.Add(new Tuple<int, string, string>(song.SongId, tags.FirstComposer, tags.FirstPerformer));
                    }
                    catch (Exception ex)
                    {
                    }
                }
                catch (Exception ex)
                {

                }
                if (count % 10 == 0)//!!!
                {
                    progress.Report(count);
                }
                count++;
            }
            DatabaseManager.UpdateComposersPerformers(newTags);
        }

        private static void SendToast()
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(loader.GetString("LibraryUpdated")));
            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        //public async static Task ImportAndUpdateDatabase()
        //{
        //    IReadOnlyList<StorageFile> list = await KnownFolders.MusicLibrary.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);

        //    foreach (var file in list)
        //    {
        //        Windows.Storage.FileProperties.BasicProperties bp = await file.GetBasicPropertiesAsync();
        //        // Sprawdzanie rozmiaru nie działa
        //        int i = await DatabaseManager.IsSongInDB(file.Path, bp.Size);
        //        if (i == -1)
        //        {
        //            SongData song = await CreateSongFromFile(file);
        //            await DatabaseManager.InsertSong(song);
        //        }
        //        else if (i == 0)
        //        {
        //            //istnieje taka sama
        //        }
        //        else if (i > 0)
        //        {
        //            SongData song = await CreateSongFromFile(file);
        //            await DatabaseManager.UpdateSongData(song, i);
        //        }
        //        else { };
        //    }
        //    OnMediaImported("Update");
        //}

        //public async static Task ImportAndCreateNewDatabase()
        //{
        //    DatabaseManager.ResetSongsTable();

        //    IReadOnlyList<StorageFile> list = await KnownFolders.MusicLibrary.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);

        //    foreach (var file in list)
        //    {
        //        SongData song = await CreateSongFromFile(file);
        //        await DatabaseManager.InsertSong(song);
        //    }
        //    OnMediaImported("NewDatabase");
        //}

        //public async static Task ImportAndCreateNewDatabase(IProgress<int> progress)
        //{
        //    DatabaseManager.ResetSongsTable();

        //    IReadOnlyList<StorageFile> list = await KnownFolders.MusicLibrary.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);
        //    int count = 1;
        //    foreach (var file in list)
        //    {
        //        SongData song = await CreateSongFromFile(file);
        //        await DatabaseManager.InsertSong(song);
        //        progress.Report(count);
        //        count++;
        //    }
        //    OnMediaImported("NewDatabase");
        //    SendToast();
        //}


    }
}
