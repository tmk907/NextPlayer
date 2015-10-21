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

        //public SongItem GetFromNowPlaying(int index)
        //{
        //    return NowPlayingList[index];
        //}

        public void ChangeRating(int rating, int songId)
        {
            nowPlayingList.ElementAt(songId).Rating = rating;
        }

        //public void SaveCurrentSongIndex(int id)
        //{
        //    int i = 0;
        //    foreach (var item in _nowPlayingList)
        //    {
        //        if (item.SongId == id)
        //        {
        //            ApplicationSettingsHelper.SaveSongIndex(i);
        //            break;
        //        }
        //        i++;
        //    }
        //}

        
        #endregion

        //public async void CreateNewPlainPlaylist(string name)
        //{
        //    int id = await DatabaseManager.InsertPlainPlaylist(name);
        //    Playlists.Add(new PlaylistItem(id,false,name));
        //}

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
                uri = new System.Uri("ms-appx:///Assets/SongCover192.png");

            }
            else
            {
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

        

        public async Task SetDB()
        {
            await DatabaseManager.CreateDatabase();
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 2);
            int i;
            i = await DatabaseManager.InsertSmartPlaylist("Ostatnio dodane", 50, SPUtility.SortBy.MostRecentlyAdded);
            await DatabaseManager.InsertSmartPlaylistEntry(i, SPUtility.Item.DateAdded, SPUtility.Comparison.IsGreater, DateTime.Now.Subtract(TimeSpan.FromDays(14)).Ticks.ToString());
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.OstatnioDodane, i);
            i = await DatabaseManager.InsertSmartPlaylist("Ostatnio odtwarzane", 50, SPUtility.SortBy.MostRecentlyPlayed);
            await DatabaseManager.InsertSmartPlaylistEntry(i, SPUtility.Item.LastPlayed, SPUtility.Comparison.IsGreater, DateTime.MinValue.Ticks.ToString());
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.OstatnioOdtwarzane, i);
            i = await DatabaseManager.InsertSmartPlaylist("Najczęściej odtwarzane",50,SPUtility.SortBy.MostOftenPlayed);
            await DatabaseManager.InsertSmartPlaylistEntry(i, SPUtility.Item.PlayCount, SPUtility.Comparison.IsGreater, "0");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.NajczesciejOdtwarzane, i);
            i = await DatabaseManager.InsertSmartPlaylist("Najlepiej oceniane",50,SPUtility.SortBy.HighestRating);
            await DatabaseManager.InsertSmartPlaylistEntry(i, SPUtility.Item.Rating, SPUtility.Comparison.IsGreater, "3");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.NajlepiejOceniane, i);
            i = await DatabaseManager.InsertSmartPlaylist("Najrzadziej odtwarzane", 50, SPUtility.SortBy.LeastOftenPlayed);
            await DatabaseManager.InsertSmartPlaylistEntry(i, SPUtility.Item.PlayCount, SPUtility.Comparison.IsGreater, "-1");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.NajrzadziejOdtwarzane, i);
            i = await DatabaseManager.InsertSmartPlaylist("Najgorzej oceniane", 50, SPUtility.SortBy.LowestRating);
            await DatabaseManager.InsertSmartPlaylistEntry(i, SPUtility.Item.Rating, SPUtility.Comparison.IsLess, "4");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.NajgorzejOceniane, i);
        }
    }

    #region stare
    #region Collections

    //private ObservableCollection<SongItem> _songs = new ObservableCollection<SongItem>();
    //public ObservableCollection<SongItem> Songs
    //{
    //    get
    //    {
    //        if (_songs.Count == 0)
    //        {
    //            GetSongsFromDB();
    //        }
    //        return _songs;
    //    }
    //}
    //private ObservableCollection<AlbumItem> _albums = new ObservableCollection<AlbumItem>();
    //public ObservableCollection<AlbumItem> Albums
    //{
    //    get
    //    {
    //        if (_albums.Count == 0) GetAlbumsFromDB();
    //        return _albums;
    //    }
    //}

    //private ObservableCollection<ArtistItem> _artists = new ObservableCollection<ArtistItem>();
    //public ObservableCollection<ArtistItem> Artists
    //{
    //    get
    //    {
    //        if (_artists.Count == 0) GetArtistsFromDB();
    //        return _artists;
    //    }
    //}
    //private ObservableCollection<GenreItem> _genres = new ObservableCollection<GenreItem>();
    //public ObservableCollection<GenreItem> Genres 
    //{
    //    get
    //    {
    //        if (_genres.Count == 0) GetGenresFromDB();
    //        return _genres;
    //    }
    //}
    //private ObservableCollection<FolderItem> _folders = new ObservableCollection<FolderItem>();
    //public ObservableCollection<FolderItem> Folders
    //{
    //    get
    //    {
    //        if (_folders.Count == 0) GetFoldersFromDB();
    //        return _folders;
    //    }
    //}
    //private ObservableCollection<PlaylistItem> _playlists = new ObservableCollection<PlaylistItem>();
    //public ObservableCollection<PlaylistItem> Playlists
    //{
    //    get
    //    {
    //        if (_playlists.Count == 0) GetPlaylistsFromDB();
    //        return _playlists;
    //    }
    //}



    #endregion
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

    //#region Get Data From DB

    //private void GetAlbumsFromDB()
    //{
    //    _albums = DatabaseManager.GetAlbumItems();
    //}

    //private void GetArtistsFromDB()
    //{
    //    _artists = DatabaseManager.GetArtistItems();
    //}

    //private void GetFoldersFromDB()
    //{

    //}

    //private void GetGenresFromDB()
    //{
    //    _genres = DatabaseManager.GetGenreItems();
    //}

    //private void GetPlaylistsFromDB()
    //{
    //    _playlists = DatabaseManager.GetPlaylistItems();
    //}



    //private void GetSongsFromDB()
    //{
    //    _songs = DatabaseManager.GetSongItems();
    //}

    //#endregion
    //#region GetGroupedData

    //public IEnumerable<GroupedOC<SongItem>> GetSongsGrouped()
    //{
    //    return Grouped.CreateGrouped<SongItem>(Songs, x => x.Title);
    //}

    //public IEnumerable<GroupedOC<SongItem>> GetSongsGrouped(string p)
    //{
    //    return Grouped.CreateGrouped<SongItem>(GetSongs(p), x => x.Title);
    //}

    //public IEnumerable<GroupedOC<AlbumItem>> GetAlbumsGrouped()
    //{
    //    return Grouped.CreateGrouped<AlbumItem>(Albums, x => x.Album);
    //}

    //public IEnumerable<GroupedOC<AlbumItem>> GetAlbumsGrouped(string artist)
    //{
    //    return Grouped.CreateGrouped<AlbumItem>(GetAlbums(artist), x => x.Album);
    //}

    //public IEnumerable<GroupedOC<ArtistItem>> GetArtistsGrouped()
    //{
    //    return Grouped.CreateGrouped<ArtistItem>(Artists, x => x.Artist);
    //}

    //#endregion


    //public List<AlbumItem> GetAlbums(string artist)
    //{
    //    return DatabaseManager.GetAlbumItems(artist);
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

    //public string GetPlaylistName(int id, bool isSmart)
    //{
    //    return Playlists.Where(p => p.Id.Equals(id)).Where(s => s.IsSmart.Equals(isSmart)).FirstOrDefault().Name;
    //}
    #endregion
}
