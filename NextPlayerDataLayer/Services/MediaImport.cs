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
using NextPlayerDataLayer.Diagnostics;

namespace NextPlayerDataLayer.Services
{
    public delegate void MediaImportedHandler(string s);

    public class OwnFileAbstraction : TagLib.File.IFileAbstraction
    {
        public OwnFileAbstraction(string name, Stream stream)
        {
            this.Name = name;
            this.ReadStream = stream;
            this.WriteStream = stream;
        }

        public OwnFileAbstraction(string name, Stream rstream, Stream wstream)
        {
            this.Name = name;
            this.ReadStream = rstream;
            this.WriteStream = wstream;
        }

        public void CloseStream(Stream stream)
        {
            stream.Flush();
        }

        public string Name
        {
            get;
            private set;
        }

        public Stream ReadStream
        {
            get;
            private set;
        }

        public Stream WriteStream
        {
            get;
            private set;
        }
    }

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
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.MediaScan, true);
            IReadOnlyList<StorageFile> list = await KnownFolders.MusicLibrary.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);
            int all = list.Count;
            int count = 1;
            Tuple<int, int> tuple;
            Dictionary<string, Tuple<int, int>> dict = DatabaseManager.GetFilePaths();

            int w1;
            int w2;

            List<int> toAvailable = new List<int>();//lista songId
            List<SongData> newSongs = new List<SongData>();

            foreach (var file in list)
            {
                string type = file.FileType.ToLower();
                if (type == ".mp3" || type == ".m4a" || type == ".wma" ||
                    type == ".wav" || type == ".aac" || type == ".asf" ||
                    type == ".adt" || type == ".adts" || type == ".amr")
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
                }
                w1 = (100 * count / all);
                w2 = (100 * (count - 1) / all);

                if (progress != null && w1!=w2)
                {
                    progress.Report(w1);
                }
                count++;
            }
            progress.Report(99);
            DatabaseManager.ChangeAvailability(toAvailable);
            Library.Current.CheckNPAfterUpdate(toAvailable);
            await DatabaseManager.InsertSongsAsync(newSongs);
            ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.MediaScan);
            OnMediaImported("Update");
            SendToast();
        }
        
        private async static Task<SongData> CreateSongFromFile(StorageFile file)
        {
            SongData song = new SongData();
            song.DateAdded = DateTime.Now;
            song.Filename = file.Name;
            song.Path = file.Path;
            song.PlayCount = 0;
            song.LastPlayed = DateTime.MinValue;
            song.IsAvailable = 1;
            song.Tag.Rating = 0;
            song.FileSize = 0;

            try
            {
                using (Stream fileStream = await file.OpenStreamForReadAsync())
                {
                    try
                    {
                        var tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
                        try
                        {
                            song.Bitrate = (uint)tagFile.Properties.AudioBitrate;
                            song.Duration = TimeSpan.FromSeconds(Convert.ToInt32(tagFile.Properties.Duration.TotalSeconds));
                        }
                        catch (Exception ex)
                        {
                            song.Duration = TimeSpan.Zero;
                            song.Bitrate = 0;
                        }
                        try
                        {
                            //TagLib.Id3v2.Tag.DefaultVersion = 3;
                            //TagLib.Id3v2.Tag.ForceDefaultVersion = true;
                            Tag tags;
                            if (tagFile.TagTypes.ToString().Contains(TagTypes.Id3v2.ToString()))
                            {
                                tags = tagFile.GetTag(TagTypes.Id3v2);
                                TagLib.Id3v2.PopularimeterFrame pop = TagLib.Id3v2.PopularimeterFrame.Get((TagLib.Id3v2.Tag)tags, "Windows Media Player 9 Series", false);
                                if (pop != null)
                                {
                                    if (224 <= pop.Rating && pop.Rating <= 255) song.Tag.Rating = 5;
                                    else if (160 <= pop.Rating && pop.Rating <= 223) song.Tag.Rating = 4;
                                    else if (96 <= pop.Rating && pop.Rating <= 159) song.Tag.Rating = 3;
                                    else if (32 <= pop.Rating && pop.Rating <= 95) song.Tag.Rating = 2;
                                    else if (1 <= pop.Rating && pop.Rating <= 31) song.Tag.Rating = 1;
                                }
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

                            song.Tag.Album = tags.Album ?? "";
                            song.Tag.AlbumArtist = tags.FirstAlbumArtist ?? "";
                            song.Tag.Artists = tags.JoinedPerformers ?? "";
                            song.Tag.Comment = tags.Comment ?? "";
                            song.Tag.Composers = tags.JoinedComposers ?? "";
                            song.Tag.Conductor = tags.Conductor ?? "";
                            song.Tag.Disc = (int)tags.Disc;
                            song.Tag.DiscCount = (int)tags.DiscCount;
                            song.Tag.FirstArtist = tags.FirstPerformer ?? "";
                            song.Tag.FirstComposer = tags.FirstComposer ?? "";
                            song.Tag.Genre = tags.FirstGenre ?? "";
                            song.Tag.Lyrics = tags.Lyrics ?? "";
                            song.Tag.Title = tags.Title ?? file.DisplayName;
                            song.Tag.Track = (int)tags.Track;
                            song.Tag.TrackCount = (int)tags.TrackCount;
                            song.Tag.Year = (int)tags.Year;
                        }
                        catch (CorruptFileException e)
                        {
                            song.Tag.Album = "";
                            song.Tag.AlbumArtist = "";
                            song.Tag.Artists = "";
                            song.Tag.Comment = "";
                            song.Tag.Composers = "";
                            song.Tag.Conductor = "";
                            song.Tag.Disc = 0;
                            song.Tag.DiscCount = 0;
                            song.Tag.FirstArtist = "";
                            song.Tag.FirstComposer = "";
                            song.Tag.Genre = "";
                            song.Tag.Lyrics = "";
                            song.Tag.Title = file.DisplayName;
                            song.Tag.Track = 0;
                            song.Tag.TrackCount = 0;
                            song.Tag.Year = 0;
                        }

                    }
                    catch (Exception ex)
                    {
                        song.Tag.Album = "";
                        song.Tag.AlbumArtist = "";
                        song.Tag.Artists = "";
                        song.Tag.Comment = "";
                        song.Tag.Composers = "";
                        song.Tag.Conductor = "";
                        song.Tag.Disc = 0;
                        song.Tag.DiscCount = 0;
                        song.Tag.FirstArtist = "";
                        song.Tag.FirstComposer = "";
                        song.Tag.Genre = "";
                        song.Tag.Lyrics = "";
                        song.Tag.Title = file.DisplayName;
                        song.Tag.Track = 0;
                        song.Tag.TrackCount = 0;
                        song.Tag.Year = 0;
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Logger.Save("CreateSongFromFile FileNotFound" + Environment.NewLine + ex.Message);
                Logger.SaveToFile();
                song.FileSize = 0;
                song.IsAvailable = 0;

                song.Duration = TimeSpan.Zero;
                song.Bitrate = 0;
                song.Tag.Album = "";
                song.Tag.AlbumArtist = "";
                song.Tag.Artists = "";
                song.Tag.Comment = "";
                song.Tag.Composers = "";
                song.Tag.Conductor = "";
                song.Tag.Disc = 0;
                song.Tag.DiscCount = 0;
                song.Tag.FirstArtist = "";
                song.Tag.FirstComposer = "";
                song.Tag.Genre = "";
                song.Tag.Lyrics = "";
                song.Tag.Title = file.DisplayName;
                song.Tag.Track = 0;
                song.Tag.TrackCount = 0;
                song.Tag.Year = 0;

                return song;
            }
            catch (Exception ex)
            {
                Logger.Save("CreateSongFromFile" + Environment.NewLine + ex.Message);
                Logger.SaveToFile();
                song.FileSize = 0;
            }
            return song;
        }

        private static void SendToast()
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(loader.GetString("LibraryUpdated")));
            ToastNotification toast = new ToastNotification(toastXml);
            try
            {
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            catch (Exception ex)
            {
                Logger.Save("SendToast LibraryUpdated" + Environment.NewLine + ex.Message);
                Logger.SaveToFile();
            }
        }

        

        //public static async Task UpdateDB(IProgress<int> progress, string dbVersion)
        //{

        //    var list = DatabaseManager.GetSongItems();
        //    int count = 0;
        //    foreach (var s in list)
        //    {
        //        try
        //        {
        //            StorageFile file = await StorageFile.GetFileFromPathAsync(s.Path);
        //            using (Stream fileStream = await file.OpenStreamForReadAsync())
        //            {
        //                try
        //                {
        //                    var tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
        //                    Tag tags;
        //                    if (tagFile.TagTypes.ToString().Contains(TagTypes.Id3v2.ToString()))
        //                    {
        //                        tags = tagFile.GetTag(TagTypes.Id3v2);
        //                    }
        //                    else if (tagFile.TagTypes.ToString().Contains(TagTypes.Id3v1.ToString()))
        //                    {
        //                        tags = tagFile.GetTag(TagTypes.Id3v1);
        //                    }
        //                    else if (tagFile.TagTypes.ToString().Contains(TagTypes.Apple.ToString()))
        //                    {
        //                        tags = tagFile.GetTag(TagTypes.Apple);
        //                    }
        //                    else
        //                    {
        //                        tags = tagFile.GetTag(tagFile.TagTypes);
        //                    }
        //                    SongData song = new SongData();
        //                    song.SongId = s.SongId;
        //                    song.Tag.Artists = tags.JoinedPerformers ?? "";
        //                    song.Tag.Comment = tags.Comment ?? "";
        //                    song.Tag.Composers = tags.JoinedComposers ?? "";
        //                    song.Tag.Conductor = tags.Conductor ?? "";
        //                    song.Tag.Disc = (int)tags.Disc;
        //                    song.Tag.DiscCount = (int)tags.DiscCount;
        //                    song.Tag.FirstArtist = tags.FirstPerformer ?? "";
        //                    song.Tag.FirstComposer = tags.FirstComposer ?? "";
        //                    song.Tag.TrackCount = (int)tags.TrackCount;

        //                    DatabaseManager.FillSongData(song);
        //                }
        //                catch (Exception ex)
        //                {

        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        if (count % 10 == 0)//!!!
        //        {
        //            progress.Report(count);
        //        }
        //        count++;
        //    }
        //    Windows.Storage.ApplicationData.Current.LocalSettings.Values[AppConstants.DBVersion] = dbVersion.Replace("notupdated", "");
        //}

    }
}
