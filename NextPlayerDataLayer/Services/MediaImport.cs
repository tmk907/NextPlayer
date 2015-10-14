﻿using System;
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
            song.Tag.Rating = 0;
            
            Stream fileStream = await file.OpenStreamForReadAsync();
            
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
                        if (pop!=null)
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
