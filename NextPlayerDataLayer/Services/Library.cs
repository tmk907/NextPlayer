using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using TagLib;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using NextPlayerDataLayer.Common;
using NextPlayerDataLayer.Constants;
using NextPlayerDataLayer.Enums;
using NextPlayerDataLayer.Helpers;
using NextPlayerDataLayer.Model;
using System.ComponentModel;
using Windows.UI.Xaml;

namespace NextPlayerDataLayer.Services
{


    public sealed class Library
    {
        private static Library instance = null;
        private Library()
        {
            MediaImport.MediaImported += new MediaImportedHandler(UpdateLibrary);
        }

        public static Library Current
        {
            get
            {
                if (instance == null)
                {
                    instance = new Library();
                }
                return instance;
            }
        }

        #region NowPlaying

        private ObservableCollection<SongItem> nowPlayingList = new ObservableCollection<SongItem>();
      
        public ObservableCollection<SongItem> NowPlayingList
        {
            get
            {
                if (nowPlayingList.Count == 0) nowPlayingList = DatabaseManager.SelectAllSongItemsFromNowPlaying();
                return nowPlayingList;
            }
        }

        public async void AddToNowPlaying(SongItem song)
        {
            await DatabaseManager.AddToNowPlayingAsync(song);
            nowPlayingList.Add(song);
        }

        public async void AddToNowPlaying(IEnumerable<SongItem> songlist)
        {
            await DatabaseManager.AddToNowPlayingAsync(songlist);
            foreach (SongItem song in songlist)
            {
                nowPlayingList.Add(song);
            }
        }

        public void ClearNowPlaying()
        {
            DatabaseManager.ClearNowPlaying();
            nowPlayingList.Clear();
        }

        public void CheckNPAfterUpdate(IEnumerable<int> toAvailable)
        {
            List<SongItem> list = new List<SongItem>();
            foreach (var song in nowPlayingList)
            {
                if (toAvailable.Contains(song.SongId))
                {
                    list.Add(song);
                }
            }
            if (list.Count <= ApplicationSettingsHelper.ReadSongIndex())
            {
                ApplicationSettingsHelper.SaveSongIndex(list.Count - 1);
            }
            SetNowPlayingList(list);
        }

        private void SaveNowPlayingInDB()
        {
            DatabaseManager.InsertNewNowPlayingPlaylist(nowPlayingList);
        }

        public void SetNowPlayingList(IEnumerable<SongItem> npList)
        {
            nowPlayingList.Clear();
            foreach(SongItem song in npList){
                nowPlayingList.Add(song);
            }
            SaveNowPlayingInDB();
        }

        public SongItem GetCurrentPlayingSong()
        {
            try
            {
                return NowPlayingList[ApplicationSettingsHelper.ReadSongIndex()];
            }
            catch(Exception ex)
            {
                Diagnostics.Logger.Save("GetCurrentPlayingSong" + Environment.NewLine + ex.Message);
                Diagnostics.Logger.SaveToFile();
            }
            return null;
        }

        public void UpdateSong(SongData updatedSong)
        {
            foreach(var song in NowPlayingList)
            {
                if(song.SongId == updatedSong.SongId)
                {
                    song.Album = updatedSong.Tag.Album;
                    song.Artist = updatedSong.Tag.Artists;
                    song.Composer = updatedSong.Tag.Composers;
                    song.Rating = (int)updatedSong.Tag.Rating;
                    song.Title = updatedSong.Tag.Title;
                    song.TrackNumber = updatedSong.Tag.Track;
                    song.Year = updatedSong.Tag.Year;
                    break;
                }
            }
        }

        public void ChangeRating(int rating, int songId)
        {
            nowPlayingList.ElementAt(songId).Rating = rating;
        }

        #endregion

        /// <summary>
        /// Return cover saved in file tags or .jpg from folder.
        /// If cover doesn't exist width and height are 0px.
        /// </summary>
        public async Task<BitmapImage> GetOriginalCover(string path, bool small)
        {
            BitmapImage bitmap = new BitmapImage();
            if (small)
            {
                bitmap.DecodePixelHeight = 192;
            }
            int a = 0;
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                Stream fileStream = await file.OpenStreamForReadAsync();
                File tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
                a = tagFile.Tag.Pictures.Length;
                fileStream.Dispose();
                if (a > 0)
                {
                    IPicture pic = tagFile.Tag.Pictures[0];
                    using (MemoryStream stream = new MemoryStream(pic.Data.Data))
                    {
                        using (Windows.Storage.Streams.IRandomAccessStream istream = stream.AsRandomAccessStream())
                        {
                            await bitmap.SetSourceAsync(istream);
                        }
                    }
                }
                else
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
                    try
                    {
                        IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                        if (files.Count > 0)
                        {
                            foreach (var x in files)
                            {
                                if (x.Path.EndsWith("jpg"))
                                {
                                    using (IRandomAccessStream stream = await x.OpenAsync(Windows.Storage.FileAccessMode.Read))
                                    {
                                        await bitmap.SetSourceAsync(stream);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
            catch (Exception e)
            {

            }
            return bitmap;
        }

        /// <summary>
        /// Return default cover.
        /// </summary>
        public async Task<BitmapImage> GetDefaultCover(bool small)
        {
            BitmapImage bitmap = new BitmapImage();
            Uri uri;
            if (small)
            {
                //if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                //{
                //    uri = new System.Uri("ms-appx:///Assets/Cover/cover-light192.png");
                //}
                //else
                //{
                //    uri = new System.Uri("ms-appx:///Assets/Cover/cover-dark192.png");
                //}
                uri = new System.Uri("ms-appx:///Assets/SongCover192.png");
            }
            else
            {
                //if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                //{
                //    uri = new System.Uri("ms-appx:///Assets/Cover/cover-light.png");
                //}
                //else
                //{
                //    uri = new System.Uri("ms-appx:///Assets/Cover/cover-dark.png");
                //}
                uri = new System.Uri("ms-appx:///Assets/SongCover.png");
            }
            var file2 = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            using (IRandomAccessStream stream = await file2.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                await bitmap.SetSourceAsync(stream);
            }
            return bitmap;
        }

        /// <summary>
        /// Return cover saved in file tags or .jpg from folder or default cover.
        /// </summary>
        public async Task<BitmapImage> GetCover(string path, bool small)
        {
            BitmapImage bitmap = new BitmapImage();
            
            bitmap = await GetOriginalCover(path, small);
            
            if (bitmap.PixelHeight == 0)
            {
                bitmap = await GetDefaultCover(small);
            }
 
            return bitmap;
        }

        public async Task<BitmapImage> GetCurrentCover(int index)
        {
            return await GetCover(NowPlayingList.ElementAt(index).Path, false);
        }

        public async Task<string> SaveAlbumCover(string album, string artist, string tileId)
        {
            string path = DatabaseManager.GetSongItemsFromAlbum(album, artist).FirstOrDefault().Path;
            string name = await SaveSongCover(path,tileId);
            return name;
        }

        public async Task<string> SaveSongCover(string path, string fileName)
        {
            string imagePath = "";
            
            int a = 0;
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                Stream fileStream = await file.OpenStreamForReadAsync();
                var tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
                a = tagFile.Tag.Pictures.Length;
               
                if (a > 0)
                {
                    IPicture pic = tagFile.Tag.Pictures[0];
                    MemoryStream stream = new MemoryStream(pic.Data.Data);

                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    StorageFile storageFile = await localFolder.CreateFileAsync(fileName+".jpg", CreationCollisionOption.ReplaceExisting);
                    imagePath = "ms-appdata:///local/" + fileName + ".jpg";
                    stream.Seek(0, SeekOrigin.Begin);
                    using (Stream outputStream = await storageFile.OpenStreamForWriteAsync())
                    {
                        stream.CopyTo(outputStream);
                    }
                }
                else
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
                    try
                    {
                        IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                        if (files.Count > 0)
                        {
                            foreach (var x in files)
                            {
                                if (x.Path.EndsWith("jpg"))
                                {
                                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                                    StorageFile storageFile = await localFolder.CreateFileAsync(fileName+".jpg", CreationCollisionOption.ReplaceExisting);
                                    imagePath = "ms-appdata:///local/"+ fileName + ".jpg";
                                    using (Stream stream = await x.OpenStreamForReadAsync())
                                    {
                                        using (Stream outputStream = await storageFile.OpenStreamForWriteAsync())
                                        {
                                            stream.CopyTo(outputStream);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
            catch (Exception e)
            {

            }
            if (imagePath=="")
            {
                imagePath = "ms-appx:///Assets/AppImages/Logo/Logo.png";
            }
            return imagePath;
        }

        private void UpdateLibrary(string s)
        {
            //if (_songs.Count != 0) GetSongsFromDB();
            //if (_albums.Count != 0) GetAlbumsFromDB();
            //if (_artists.Count != 0) GetArtistsFromDB();
            //if (_folders.Count != 0) GetFoldersFromDB();
            //if (_genres.Count != 0) GetGenresFromDB();
            //if (_playlists.Count != 0) GetPlaylistsFromDB();
        }

        

        public void SetDB()
        {
            DatabaseManager.CreateDatabase();
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 2);
            int i;
            i = DatabaseManager.InsertSmartPlaylist2("Ostatnio dodane", 50, SPUtility.SortBy.MostRecentlyAdded);
            DatabaseManager.InsertSmartPlaylistEntry2(i, SPUtility.Item.DateAdded, SPUtility.Comparison.IsGreater, DateTime.Now.Subtract(TimeSpan.FromDays(14)).Ticks.ToString());
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.OstatnioDodane, i);
            i = DatabaseManager.InsertSmartPlaylist2("Ostatnio odtwarzane", 50, SPUtility.SortBy.MostRecentlyPlayed);
            DatabaseManager.InsertSmartPlaylistEntry2(i, SPUtility.Item.LastPlayed, SPUtility.Comparison.IsGreater, DateTime.MinValue.Ticks.ToString());
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.OstatnioOdtwarzane, i);
            i = DatabaseManager.InsertSmartPlaylist2("Najczęściej odtwarzane",50,SPUtility.SortBy.MostOftenPlayed);
            DatabaseManager.InsertSmartPlaylistEntry2(i, SPUtility.Item.PlayCount, SPUtility.Comparison.IsGreater, "0");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.NajczesciejOdtwarzane, i);
            i = DatabaseManager.InsertSmartPlaylist2("Najlepiej oceniane",50,SPUtility.SortBy.HighestRating);
            DatabaseManager.InsertSmartPlaylistEntry2(i, SPUtility.Item.Rating, SPUtility.Comparison.IsGreater, "3");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.NajlepiejOceniane, i);
            i = DatabaseManager.InsertSmartPlaylist2("Najrzadziej odtwarzane", 50, SPUtility.SortBy.LeastOftenPlayed);
            DatabaseManager.InsertSmartPlaylistEntry2(i, SPUtility.Item.PlayCount, SPUtility.Comparison.IsGreater, "-1");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.NajrzadziejOdtwarzane, i);
            i = DatabaseManager.InsertSmartPlaylist2("Najgorzej oceniane", 50, SPUtility.SortBy.LowestRating);
            DatabaseManager.InsertSmartPlaylistEntry2(i, SPUtility.Item.Rating, SPUtility.Comparison.IsLess, "4");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.NajgorzejOceniane, i);
        }
    }

    #region stare

    //public ObservableCollection<int> AllSongsId;
    //public async Task<WriteableBitmap> GetCoverForBG(string path)
    //{
    //    int a = 0;
    //    WriteableBitmap bitmap = BitmapFactory.New(1, 1);
    //    WriteableBitmap b2 = BitmapFactory.New(1, 1);
    //    using (bitmap.GetBitmapContext())
    //    {
    //        try
    //        {
    //            StorageFile file = await StorageFile.GetFileFromPathAsync(path);
    //            Stream fileStream = await file.OpenStreamForReadAsync();
    //            var tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
    //            a = tagFile.Tag.Pictures.Length;

    //            if (a > 0)
    //            {
    //                IPicture pic = tagFile.Tag.Pictures[0];
    //                using (MemoryStream inStream = new MemoryStream(pic.Data.Data))
    //                {
    //                    using (Windows.Storage.Streams.IRandomAccessStream raStream = inStream.AsRandomAccessStream())
    //                    {
    //                        bitmap = await b2.FromStream(raStream);
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
    //                try
    //                {
    //                    IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
    //                    if (files.Count > 0)
    //                    {
    //                        foreach (var x in files)
    //                        {
    //                            if (x.Path.EndsWith("jpg"))
    //                            {
    //                                using (IRandomAccessStream stream = await x.OpenAsync(Windows.Storage.FileAccessMode.Read))
    //                                {
    //                                    bitmap = await b2.FromStream(stream);
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //                catch (Exception e)
    //                {

    //                }
    //            }
    //        }
    //        catch (Exception e)
    //        {

    //        }
    //        if (bitmap.PixelHeight == 1)
    //        {
    //            var uri = new System.Uri("ms-appx:///Assets/SongCover.png");
    //            //var file2 = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
    //            //using (IRandomAccessStream stream = await file2.OpenAsync(Windows.Storage.FileAccessMode.Read))
    //            //{

    //            //}
    //            bitmap = await b2.FromContent(uri);
    //        }

    //        bitmap.AdjustBrightness(-80);
    //    }

    //    return bitmap;
    //}

    //public ObservableCollection<SongItem> GetSongs(string parameter)
    //{
    //    ObservableCollection<SongItem> collection = new ObservableCollection<SongItem>();
    //    string[] array = parameter.Split('|');
    //    if (array[0].Equals("album"))
    //    {
    //        collection = DatabaseManager.GetSongItemsFromAlbum(array[1],array[2]);
    //    }
    //    else if (array[0].Equals("genre"))
    //    {
    //        collection = DatabaseManager.GetSongItemsFromGenre(array[1]);
    //    }
    //    else if (array[0].Equals("smart"))
    //    {
    //        collection = DatabaseManager.GetSongItemsFromSmartPlaylist(Int32.Parse(array[1]));
    //    }
    //    else if (array[0].Equals("plain"))
    //    {
    //        collection = DatabaseManager.GetSongItemsFromPlainPlaylist(Int32.Parse(array[1]));
    //    }

    //    return collection;
    //}

    #endregion
}
