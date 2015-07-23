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

namespace NextPlayerDataLayer.Services
{
    

    public sealed class Library
    {
        private static readonly Library instance = new Library();
        private Library()
        {
            MediaImport.MediaImported += new MediaImportedHandler(UpdateLibrary);
        }

        public static Library Current
        {
            get
            {
                return instance;
            }
        }

        #region Collections

        private ObservableCollection<SongItem> _songs = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Songs
        {
            get
            {
                if (_songs.Count == 0)
                {
                    GetSongsFromDB();
                }
                return _songs;
            }
        }
        private ObservableCollection<AlbumItem> _albums = new ObservableCollection<AlbumItem>();
        public ObservableCollection<AlbumItem> Albums
        {
            get
            {
                if (_albums.Count == 0) GetAlbumsFromDB();
                return _albums;
            }
        }

        private ObservableCollection<ArtistItem> _artists = new ObservableCollection<ArtistItem>();
        public ObservableCollection<ArtistItem> Artists
        {
            get
            {
                if (_artists.Count == 0) GetArtistsFromDB();
                return _artists;
            }
        }
        private ObservableCollection<GenreItem> _genres = new ObservableCollection<GenreItem>();
        public ObservableCollection<GenreItem> Genres 
        {
            get
            {
                if (_genres.Count == 0) GetGenresFromDB();
                return _genres;
            }
        }
        private ObservableCollection<FolderItem> _folders = new ObservableCollection<FolderItem>();
        public ObservableCollection<FolderItem> Folders
        {
            get
            {
                if (_folders.Count == 0) GetFoldersFromDB();
                return _folders;
            }
        }
        private ObservableCollection<PlaylistItem> _playlists = new ObservableCollection<PlaylistItem>();
        public ObservableCollection<PlaylistItem> Playlists
        {
            get
            {
                if (_playlists.Count == 0) GetPlaylistsFromDB();
                return _playlists;
            }
        }

        private ObservableCollection<SongItem> _nowPlayingList = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> NowPlayingList
        {
            get
            {
                if (_nowPlayingList.Count == 0) GetNowPlayingListFromDB();
                return _nowPlayingList;
            }
        }

        #endregion
        //public ObservableCollection<int> AllSongsId;

        #region Get Data From DB

        private void GetAlbumsFromDB()
        {
            _albums = DatabaseManager.GetAlbumItems();
        }

        private void GetArtistsFromDB()
        {
            _artists = DatabaseManager.GetArtistItems();
        }

        private void GetFoldersFromDB()
        {

        }

        private void GetGenresFromDB()
        {
            _genres = DatabaseManager.GetGenreItems();
        }

        private void GetPlaylistsFromDB()
        {
            _playlists = DatabaseManager.GetPlaylistItems();
        }

        private void GetNowPlayingListFromDB()
        {
            _nowPlayingList = DatabaseManager.SelectAllSongItemsFromNowPlaying();
        }

        private void GetSongsFromDB()
        {
            _songs = DatabaseManager.GetSongItems();
        }

        #endregion

        #region GetGroupedData

        public IEnumerable<GroupedOC<SongItem>> GetSongsGrouped()
        {
            return Grouped.CreateGrouped<SongItem>(Songs, x => x.Title);
        }

        public IEnumerable<GroupedOC<SongItem>> GetSongsGrouped(string p)
        {
            return Grouped.CreateGrouped<SongItem>(GetSongs(p), x => x.Title);
        }

        public IEnumerable<GroupedOC<AlbumItem>> GetAlbumsGrouped()
        {
            return Grouped.CreateGrouped<AlbumItem>(Albums, x => x.Album);
        }

        public IEnumerable<GroupedOC<AlbumItem>> GetAlbumsGrouped(string artist)
        {
            return Grouped.CreateGrouped<AlbumItem>(GetAlbums(artist), x => x.Album);
        }

        public IEnumerable<GroupedOC<ArtistItem>> GetArtistsGrouped()
        {
            return Grouped.CreateGrouped<ArtistItem>(Artists, x => x.Artist);
        }

        #endregion
        

        public List<AlbumItem> GetAlbums(string artist)
        {
            return DatabaseManager.GetAlbumItems(artist);
        }

        public ObservableCollection<SongItem> GetSongs(string parameter)
        {
            ObservableCollection<SongItem> collection = new ObservableCollection<SongItem>();
            string[] array = parameter.Split('|');
            if (array[0].Equals("album"))
            {
                collection = DatabaseManager.GetSongItemsFromAlbum(array[1],array[2]);
            }
            else if (array[0].Equals("genre"))
            {
                collection = DatabaseManager.GetSongItemsFromGenre(array[1]);
            }
            else if (array[0].Equals("smart"))
            {
                collection = DatabaseManager.GetSongItemsFromSmartPlaylist(Int32.Parse(array[1]));
            }
            else if (array[0].Equals("plain"))
            {
                collection = DatabaseManager.GetSongItemsFromPlainPlaylist(Int32.Parse(array[1]));
            }
            
            return collection;
        }

        public string GetPlaylistName(int id, bool isSmart)
        {
            return Playlists.Where(p => p.Id.Equals(id)).Where(s => s.IsSmart.Equals(isSmart)).FirstOrDefault().Name;
        }

        #region NowPlaying

        /// <summary>
        /// cuurent playing song index+1
        /// </summary>
        public int NowPlayingSongNumber
        {
            get
            {
                object index = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.SongIndex);
                if (index != null)
                {
                    int i = Int32.Parse(index.ToString());
                    return i + 1;
                }
                else return -1;
            }
        }

        public int NowPlayingCount
        {
            get
            {
                if (_nowPlayingList.Count == 0) GetNowPlayingListFromDB();
                return _nowPlayingList.Count;
            }
        }

        public void AddToNowPlaying(SongItem song)
        {
            _nowPlayingList.Add(song);
        }

        public void AddToNowPlaying(IEnumerable<SongItem> songlist)
        {
            foreach (SongItem song in songlist)
            {
                _nowPlayingList.Add(song);
            }
        }

        public void ClearNowPlaying()
        {
            DatabaseManager.ClearNowPlaying();
            _nowPlayingList.Clear();
        }

        private void SaveNowPlayingInDB()
        {
            DatabaseManager.InsertNewNowPlayingPlaylist(NowPlayingList);
        }

        public void SetNowPlayingList(IEnumerable<SongItem> npList)
        {
            _nowPlayingList.Clear();
            foreach(SongItem song in npList){
                _nowPlayingList.Add(song);
            }
            SaveNowPlayingInDB();
        }

        public void SaveCurrentSongIndex(int id)
        {
            int i = 0;
            foreach (var item in _nowPlayingList)
            {
                if (item.SongId == id)
                {
                    ApplicationSettingsHelper.SaveSongIndex(i);
                    break;
                }
                i++;
            }
        }

        public SongItem GetFromNowPlaying(int index)
        {
            return NowPlayingList[index];
        }

        #endregion

        public async void CreateNewPlainPlaylist(string name)
        {
            int id = await DatabaseManager.InsertPlainPlaylist(name);
            Playlists.Add(new PlaylistItem(id,false,name));
        }


        private async Task<BitmapImage> GetCover(string path)
        {
            BitmapImage bitmap = new BitmapImage();
            int a = 0;
            //try
            //{
            StorageFile file = await StorageFile.GetFileFromPathAsync(path);
            Stream fileStream = await file.OpenStreamForReadAsync();
            var tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
            a = tagFile.Tag.Pictures.Length;
            //}
            //catch(Exception e){

            //}
            if (a > 0)
            {
                IPicture pic = tagFile.Tag.Pictures[0];
                MemoryStream stream = new MemoryStream(pic.Data.Data);
                Windows.Storage.Streams.IRandomAccessStream istream = stream.AsRandomAccessStream();
                await bitmap.SetSourceAsync(istream);
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
            return bitmap;
        }

        public async Task<BitmapImage> GetCurrentCover(int index)
        {
            return await GetCover(NowPlayingList.ElementAt(index).Path);
        }

        public async Task<BitmapImage> GetAlbumCover(string album)
        {
            string path = Songs.Where(s => s.Album.Equals(album)).Select(p => p.Path).FirstOrDefault();
            return await GetCover(path);
        }

        private void UpdateLibrary(string s)
        {
            if (_songs.Count != 0) GetSongsFromDB();
            if (_albums.Count != 0) GetAlbumsFromDB();
            if (_artists.Count != 0) GetArtistsFromDB();
            if (_folders.Count != 0) GetFoldersFromDB();
            if (_genres.Count != 0) GetGenresFromDB();
            if (_playlists.Count != 0) GetPlaylistsFromDB();
        }

        public async Task SetDB()
        {
            await DatabaseManager.CreateDatabase();
            int i;
            i = await DatabaseManager.InsertSmartPlaylist("Ostatnio dodane", 50, SPUtility.SortBy.MostRecentlyAdded);
            await DatabaseManager.InsertSmartPlaylistEntry(i, SPUtility.Item.DateAdded, SPUtility.Comparison.IsGreater, DateTime.Now.Subtract(TimeSpan.FromDays(14)).Ticks.ToString());
            i = await DatabaseManager.InsertSmartPlaylist("Ostatnio odtwarzane", 50, SPUtility.SortBy.MostRecentlyPlayed);
            await DatabaseManager.InsertSmartPlaylistEntry(i, SPUtility.Item.LastPlayed, SPUtility.Comparison.IsGreater, DateTime.MinValue.Ticks.ToString());
            i = await DatabaseManager.InsertSmartPlaylist("Najczęściej odtwarzane",50,SPUtility.SortBy.MostOftenPlayed);
            await DatabaseManager.InsertSmartPlaylistEntry(i, SPUtility.Item.PlayCount, SPUtility.Comparison.IsGreater, "0");
            i = await DatabaseManager.InsertSmartPlaylist("Najlepiej oceniane",50,SPUtility.SortBy.HighestRating);
            await DatabaseManager.InsertSmartPlaylistEntry(i, SPUtility.Item.Rating, SPUtility.Comparison.IsGreater, "4");
        }
    }
}
