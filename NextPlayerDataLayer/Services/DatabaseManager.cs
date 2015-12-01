using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using System.IO;
using Windows.Storage;
using NextPlayerDataLayer.Constants;
using NextPlayerDataLayer.Enums;
using NextPlayerDataLayer.Helpers;
using NextPlayerDataLayer.Model;
using NextPlayerDataLayer.Tables;
using System.Diagnostics;

namespace NextPlayerDataLayer.Services
{
    public class DatabaseManager
    {

        private static SQLiteAsyncConnection AsyncConnectionDb()
        {
            var conn = new SQLite.SQLiteAsyncConnection(Path.Combine(ApplicationData.Current.LocalFolder.Path, AppConstants.DBFileName), true);
            return conn;
        }

        private static SQLiteConnection ConnectionDb()
        {
            var conn = new SQLite.SQLiteConnection(Path.Combine(ApplicationData.Current.LocalFolder.Path, AppConstants.DBFileName), true);
            conn.BusyTimeout = TimeSpan.FromSeconds(2);
            return conn;
        }

        //Only available songs
        private static SQLite.TableQuery<SongsTable2> SongsConn()
        {
            return ConnectionDb().Table<SongsTable2>().Where(available => available.IsAvailable > 0);
        }

        private static SQLite.AsyncTableQuery<SongsTable2> SongsConnAsync()
        {
            return AsyncConnectionDb().Table<SongsTable2>().Where(available => available.IsAvailable > 0);
        }

        public async static Task DeleteDatabase()
        {
            await AsyncConnectionDb().DropTableAsync<SongsTable2>();
            await AsyncConnectionDb().DropTableAsync<PlainPlaylistEntryTable>();
            await AsyncConnectionDb().DropTableAsync<PlainPlaylistsTable>();
            await AsyncConnectionDb().DropTableAsync<SmartPlaylistEntryTable>();
            await AsyncConnectionDb().DropTableAsync<SmartPlaylistsTable>();
            await AsyncConnectionDb().DropTableAsync<NowPlayingTable>();
        }

        public static async Task CreateDatabase()
        {
            await AsyncConnectionDb().CreateTableAsync<PlainPlaylistsTable>();
            await AsyncConnectionDb().CreateTableAsync<PlainPlaylistEntryTable>();
            await AsyncConnectionDb().CreateTableAsync<SmartPlaylistsTable>();
            await AsyncConnectionDb().CreateTableAsync<SmartPlaylistEntryTable>();
            await AsyncConnectionDb().CreateTableAsync<SongsTable2>();
            await AsyncConnectionDb().CreateTableAsync<NowPlayingTable>();
        }

        //public static void ResetSongsTable()
        //{
        //    ConnectionDb().DropTable<SongsTable>();
        //    ConnectionDb().CreateTable<SongsTable>();
        //}

        public static void UpdateDBToVersion2()
        {
            var conn = ConnectionDb();
            conn.CreateTable<SongsTable2>();
            //List<SongsTable2> songs = new List<SongsTable2>();
            //var oldSongsTable = conn.Table<SongsTable>().Where(s=>s.IsAvailable > 0);
            //foreach (var s in oldSongsTable)
            //{
            //    songs.Add(new SongsTable2()
            //    {
            //        Album = s.Album,
            //        AlbumArtist = s.AlbumArtist,
            //        Artists = "",
            //        Bitrate = s.Bitrate,
            //        Comment = "",
            //        Composers = "",
            //        Conductor = "",
            //        DateAdded = s.DateAdded,
            //        Disc = 0,
            //        DiscCount = 0,
            //        Duration = s.Duration,
            //        Filename = s.Filename,
            //        FileSize = s.FileSize,
            //        FirstArtist = "",
            //        FirstComposer = "",
            //        Genre = s.Genre,
            //        IsAvailable = s.IsAvailable,
            //        LastPlayed = s.LastPlayed,
            //        Lyrics = s.Lyrics,
            //        Path = s.Path,
            //        PlayCount = s.PlayCount,
            //        Rating = s.Rating,
            //        SongId = s.SongId,
            //        Title = s.Title,
            //        Track = (int)s.TrackNumber,
            //        TrackCount = 0,
            //        Year = (int)s.Year,
            //    });
            //}
            //conn.InsertAll(songs);

            conn.DropTable<SongsTable>();
            conn.DropTable<NowPlayingTable>();
            conn.CreateTable<NowPlayingTable>();
            conn.DropTable<PlainPlaylistEntryTable>();
            conn.CreateTable<PlainPlaylistEntryTable>();
        }

        #region Insert

        public static int InsertPlainPlaylist(string _name)
        {
            var newplaylist = new PlainPlaylistsTable
            {
                Name = _name,
            };

            ConnectionDb().Insert(newplaylist);
            return newplaylist.PlainPlaylistId;
        }

        public async static Task InsertPlainPlaylistEntry(int _playlistId, int _songId, int _place)
        {
            var newEntry = new PlainPlaylistEntryTable
            {
                PlaylistId = _playlistId,
                SongId = _songId,
                Place = _place,
            };

            await AsyncConnectionDb().InsertAsync(newEntry);
        }

        public async static Task<int> InsertSmartPlaylist(string _name, int _number, string _sorting)
        {
            var newplaylist = new SmartPlaylistsTable
            {
                Name = _name,
                SongsNumber = _number,
                SortBy = _sorting,
            };

            await AsyncConnectionDb().InsertAsync(newplaylist);
            return newplaylist.SmartPlaylistId;
        }

        public async static Task InsertSmartPlaylistEntry(int _playlistId, string _item, string _comparison, string _value)
        {
            var newEntry = new SmartPlaylistEntryTable
            {
                PlaylistId = _playlistId,
                Item = _item,
                Comparison = _comparison,
                Value = _value,
            };

            await AsyncConnectionDb().InsertAsync(newEntry);
        }

        public async static Task<int> InsertSong(SongData song)
        {
            SongsTable2 newSong = CreateSongsTable(song);
            await AsyncConnectionDb().InsertAsync(newSong);
            return newSong.SongId;
        }

        public async static Task InsertSongsAsync(IEnumerable<SongData> list)
        {
            List<SongsTable2> newSongs = new List<SongsTable2>();
            foreach (var item in list)
            {
                newSongs.Add(CreateSongsTable(item));
            }
            await AsyncConnectionDb().InsertAllAsync(newSongs);
        }

        public static void InsertNewNowPlayingPlaylist(IEnumerable<SongItem> list)
        {
            ClearNowPlaying();
            int i = 0;
            SQLiteConnection db = ConnectionDb();
            List<NowPlayingTable> list2 = new List<NowPlayingTable>();
            foreach (var e in list)
            {
                var newSong = new NowPlayingTable()
                {
                    Artist = e.Artist,
                    Path = e.Path,
                    Position = i,
                    SongId = e.SongId,
                    Title = e.Title,
                };
                list2.Add(newSong);
                i++;
            }
            db.InsertAll(list2);
        }

        public async static Task AddToNowPlayingAsync(SongItem song)
        {
            var conn = AsyncConnectionDb();
            var count = await conn.Table<NowPlayingTable>().CountAsync();
            var newSong = new NowPlayingTable()
            {
                Artist = song.Artist,
                Path = song.Path,
                Position = count,
                SongId = song.SongId,
                Title = song.Title,
            };
            await conn.InsertAsync(newSong);
            NPChange.SendMessageNPChanged();
        }

        public async static Task AddToNowPlayingAsync(IEnumerable<SongItem> songList)
        {
            var conn = AsyncConnectionDb();
            var count = await conn.Table<NowPlayingTable>().CountAsync();
            List<NowPlayingTable> list2 = new List<NowPlayingTable>();
            foreach (var e in songList)
            {
                var newSong = new NowPlayingTable()
                {
                    Artist = e.Artist,
                    Path = e.Path,
                    Position = count,
                    SongId = e.SongId,
                    Title = e.Title,
                };
                list2.Add(newSong);
                count++;
            }
            await conn.InsertAllAsync(list2);
            NPChange.SendMessageNPChanged();
        }

        public static void AddSongToPlaylist(int songId, int playlistId)
        {
            var conn = ConnectionDb();
            int lastPosition = conn.Table<PlainPlaylistEntryTable>().Where(p => p.PlaylistId == playlistId).ToList().Count;
            var item = new PlainPlaylistEntryTable(){
                PlaylistId = playlistId,
                SongId = songId,
                Place = lastPosition + 1,
            };
            conn.Insert(item);
        }

        public async static Task AddFolderToPlaylistAsync(string directory, int playlistId)
        {
            var conn = ConnectionDb();
            var query = await SongsConnAsync().OrderBy(s=>s.Title).ToListAsync();
            List<PlainPlaylistEntryTable> list = new List<PlainPlaylistEntryTable>();
            int lastPosition = conn.Table<PlainPlaylistEntryTable>().Where(p => p.PlaylistId == playlistId).ToList().Count;
            foreach (var item in query)
            {
                if (Path.GetDirectoryName(item.Path).Equals(directory))
                {
                    lastPosition++;
                    var newEntry = new PlainPlaylistEntryTable()
                    {
                        PlaylistId = playlistId,
                        SongId = item.SongId,
                        Place = lastPosition,
                    };
                    list.Add(newEntry);
                }
            }
            conn.InsertAll(list);
        }

        public async static Task AddGenreToPlaylistAsync(string genre, int playlistId)
        {
            var conn = ConnectionDb();
            var query = await SongsConnAsync().Where(s => s.Genre.Equals(genre)).ToListAsync();
            List<PlainPlaylistEntryTable> list = new List<PlainPlaylistEntryTable>();
            int lastPosition = conn.Table<PlainPlaylistEntryTable>().Where(p => p.PlaylistId == playlistId).ToList().Count;
            foreach (var item in query)
            {
                lastPosition++;
                var newEntry = new PlainPlaylistEntryTable()
                {
                    PlaylistId = playlistId,
                    SongId = item.SongId,
                    Place = lastPosition,
                };
                list.Add(newEntry);
            }
            conn.InsertAll(list);
        }

        public async static Task AddArtistToPlaylistAsync(string artistParam, int playlistId)
        {
            var conn = ConnectionDb();
            var query = await SongsConnAsync().Where(s => s.Artists.Equals(artistParam)).ToListAsync();
            List<PlainPlaylistEntryTable> list = new List<PlainPlaylistEntryTable>();
            int lastPosition = conn.Table<PlainPlaylistEntryTable>().Where(p => p.PlaylistId == playlistId).ToList().Count;
            query.OrderBy(s => s.Album).ThenBy(t => t.Track);
            foreach (var item in query)
            {
                lastPosition++;
                var newEntry = new PlainPlaylistEntryTable()
                {
                    PlaylistId = playlistId,
                    SongId = item.SongId,
                    Place = lastPosition,
                };
                list.Add(newEntry);
            }
            conn.InsertAll(list);
        }

        public async static Task AddAlbumToPlaylistAsync(string album, string artist, int playlistId)
        {
            var conn = ConnectionDb();
            var songs = GetSongItemsFromAlbum(album, artist);
            List<PlainPlaylistEntryTable> list = new List<PlainPlaylistEntryTable>();
            int lastPosition = conn.Table<PlainPlaylistEntryTable>().Where(p => p.PlaylistId == playlistId).ToList().Count;
            foreach (var item in songs)
            {
                lastPosition++;
                var newEntry = new PlainPlaylistEntryTable()
                {
                    PlaylistId = playlistId,
                    SongId = item.SongId,
                    Place = lastPosition,
                };
                list.Add(newEntry);
            }
            conn.InsertAll(list);
        }

        public static async Task AddNowPlayingToPlaylist(int playlistId)
        {
            var conn = ConnectionDb();
            var songs = await AsyncConnectionDb().Table<NowPlayingTable>().ToListAsync();
            List<PlainPlaylistEntryTable> list = new List<PlainPlaylistEntryTable>();
            int lastPosition = conn.Table<PlainPlaylistEntryTable>().Where(p => p.PlaylistId == playlistId).ToList().Count;
            foreach (var item in songs)
            {
                lastPosition++;
                var newEntry = new PlainPlaylistEntryTable()
                {
                    PlaylistId = playlistId,
                    SongId = item.SongId,
                    Place = lastPosition,
                };
                list.Add(newEntry);
            }
            conn.InsertAll(list);
        }
        #endregion

        #region Delete

        public async static void DeletePlainPlaylist(int _playlistId) 
        {
            var items = await AsyncConnectionDb().Table<PlainPlaylistEntryTable>().Where(e => e.PlaylistId.Equals(_playlistId)).ToListAsync();
            foreach (var item in items)
            {
                DeletePlainPlaylistEntry(item.Id);
            }
            var playlist = ConnectionDb().Table<PlainPlaylistsTable>().Where(p => p.PlainPlaylistId.Equals(_playlistId)).FirstOrDefault();
            ConnectionDb().Delete(playlist);
        }

        public static void DeletePlainPlaylistEntry(int primaryId)
        {
            ConnectionDb().Delete<PlainPlaylistEntryTable>(primaryId);
        }

        public static void DeletePlainPlaylistEntryById(int songId)
        {
            var conn = ConnectionDb();
            var item = conn.Table<PlainPlaylistEntryTable>().Where(s=>s.SongId==songId).FirstOrDefault();
            conn.Delete<PlainPlaylistEntryTable>(item.Id);
        }

        public async static void DeleteSmartPlaylist(int id)
        {
            var items = await AsyncConnectionDb().Table<SmartPlaylistEntryTable>().Where(e => e.PlaylistId.Equals(id)).ToListAsync();
            foreach (var item in items)
            {
                DeleteSmartPlaylistEntry(item.Id);
            }
            var playlist = ConnectionDb().Table<SmartPlaylistsTable>().Where(p => p.SmartPlaylistId.Equals(id)).FirstOrDefault();
            ConnectionDb().Delete(playlist);
        }

        public static void DeleteSmartPlaylistEntry(int primaryId)
        {
            ConnectionDb().Delete<SmartPlaylistEntryTable>(primaryId);
        }

        public async static Task DeleteSong(int _songId)
        {
            var song = await AsyncConnectionDb().Table<SongsTable2>().Where(s => s.SongId.Equals(_songId)).FirstOrDefaultAsync();
            await AsyncConnectionDb().DeleteAsync(song);
        }

        public static void ClearNowPlaying()
        {
            ConnectionDb().DeleteAll<NowPlayingTable>();
            NPChange.SendMessageNPChanged();

        }

        #endregion

        #region Get

        public static ObservableCollection<ArtistItem> GetArtistItems()
        {
            ObservableCollection<ArtistItem> collection = new ObservableCollection<ArtistItem>();
            var query = SongsConn().OrderBy(s => s.Artists).GroupBy(s => s.Artists).ToList();
            foreach (var artist in query)
            {
                ArtistItem artistItem = new ArtistItem();
                string name = artist.FirstOrDefault().Artists;
                int songs = 0;
                int albums = 0;
                TimeSpan duration = TimeSpan.Zero;
                foreach (var item in artist.GroupBy(e => e.Album).ToList())
                {
                    foreach (var song in item)
                    {
                        duration += song.Duration;
                        songs++;
                    }
                    albums++;
                }
                collection.Add(new ArtistItem(albums, name, duration, songs));
            }
            return collection;
        }

        public async static Task<ObservableCollection<ArtistItem>> GetArtistItemsAsync()
        {
            var query = await SongsConnAsync().OrderBy(s => s.Artists).ToListAsync();
            ObservableCollection<ArtistItem> collection = new ObservableCollection<ArtistItem>();
            var result = query.GroupBy(s => s.Artists);
            foreach (var artist in result)
            {
                ArtistItem artistItem = new ArtistItem();
                string name = artist.FirstOrDefault().Artists;
                int songs = 0;
                int albums = 0;
                TimeSpan duration = TimeSpan.Zero;
                foreach (var item in artist.GroupBy(e => e.Album).ToList())
                {
                    foreach (var song in item)
                    {
                        duration += song.Duration;
                        songs++;
                    }
                    albums++;
                }
                collection.Add(new ArtistItem(albums, name, duration, songs));
            }
            return collection;
        }

        public static ObservableCollection<AlbumItem> GetAlbumItems()
        {
            ObservableCollection<AlbumItem> collection = new ObservableCollection<AlbumItem>();
            var query = SongsConn().OrderBy(a => a.Album).GroupBy(g => g.Album).ToList();
            foreach (var item in query)
            {
                TimeSpan duration = TimeSpan.Zero;
                int songs = 0;
                string albumArtist = "";
                foreach (var song in item)
                {
                    if (song.AlbumArtist != "") albumArtist = song.AlbumArtist;
                    duration += song.Duration;
                    songs++;
                }
                if (albumArtist == "")
                {
                    if (item.Count() == 1)
                    {
                        albumArtist = item.FirstOrDefault().Artists;
                    }
                    else
                    {
                        var first = item.FirstOrDefault().Artists;
                        bool dif = false;
                        foreach (var song in item)
                        {
                            if (song.Artists != first)
                            {
                                dif = true;
                                break;
                            }
                        }
                        if (!dif) albumArtist = first;
                    }
                }
                if (item.FirstOrDefault().Album == "" && item.Count() > 1)
                {
                    albumArtist = "";
                }
                collection.Add(new AlbumItem(item.FirstOrDefault().Album, null, albumArtist, duration, songs));
            }
            return collection;
        }

        public static List<AlbumItem> GetAlbumItems(string artistParam)
        {
            List<AlbumItem> collection = new List<AlbumItem>();
            var query = SongsConn().Where(a => a.Artists.Equals(artistParam)).OrderBy(o => o.Album).GroupBy(g => g.Album).ToList();
            foreach (var item in query)
            {
                TimeSpan duration = TimeSpan.Zero;
                int songs = 0;
                string albumArtist = "";
                foreach (var song in item)
                {
                    if (song.AlbumArtist != "") albumArtist = song.AlbumArtist;
                    duration += song.Duration;
                    songs++;
                }
                if (albumArtist == "")
                {
                    if (item.Count() == 1)
                    {
                        albumArtist = item.FirstOrDefault().Artists;
                    }
                    else
                    {
                        albumArtist = artistParam;
                    }
                }
                collection.Add(new AlbumItem(item.FirstOrDefault().Album, artistParam, albumArtist, duration, songs));
            }
            return collection;
        }

        public static AlbumItem GetAlbumItem(string albumParam, string artistParam)
        {
            List<SongsTable2> query;
            if (artistParam == null)
            {
                query = SongsConn().Where(a => a.Album.Equals(albumParam)).ToList();
            }
            else
            {
                query = SongsConn().Where(a => a.Album.Equals(albumParam)).Where(b => b.Artists.Equals(artistParam)).ToList();
            }
            
            TimeSpan duration = TimeSpan.Zero;
            int songs = 0;
            string albumArtist = "";
            foreach (var song in query)
            {
                if (song.AlbumArtist != "") albumArtist = song.AlbumArtist;
                songs++;
                duration += song.Duration;
            }
            string artist = query.FirstOrDefault().Artists;
            if (albumParam == "" && (artistParam == null || artistParam == "")) //unknown album and artist
            {
                albumArtist = "";
                artist = "";
            }
            AlbumItem albumItem = new AlbumItem(albumParam, artist, albumArtist, duration, songs);
            //AlbumItem albumItem = new AlbumItem(query.FirstOrDefault().Album, query.FirstOrDefault().Artists, albumArtist, duration, songs);
            return albumItem;
        }

        public static ObservableCollection<FolderItem> GetFolderItems()
        {
            List<FolderItem> collection = new List<FolderItem>();
            var query = SongsConn().GroupBy(e => Path.GetDirectoryName(e.Path)).ToList();
            foreach (var item in query)
            {
                int songs = 0;
                foreach (var song in item)
                {
                    songs++;
                }
                string path = item.FirstOrDefault().Path;
                string dir = (Path.GetDirectoryName(path));
                string name = Path.GetFileName(dir);
                if (name == "") name = dir;
                //string b = Path.GetDirectoryName(path);
                collection.Add(new FolderItem(name, dir, songs));
            }
            ObservableCollection<FolderItem> result = new ObservableCollection<FolderItem>();
            foreach (var x in collection.OrderBy(f => f.Directory.ToLower()))
            {
                result.Add(x);
            }
            return result;
        }

        public async static Task<ObservableCollection<FolderItem>> GetFolderItemsAsync()
        {
            var query = await SongsConnAsync().ToListAsync();
            List<FolderItem> list = new List<FolderItem>();
            ObservableCollection<FolderItem> collection = new ObservableCollection<FolderItem>();
            var result = query.GroupBy(e => Path.GetDirectoryName(e.Path));
            foreach (var item in result)
            {
                int songs = 0;
                foreach (var song in item)
                {
                    songs++;
                }
                string path = item.FirstOrDefault().Path;
                string dir = (Path.GetDirectoryName(path));
                string name = Path.GetFileName(dir);
                if (name == "") name = dir;
                list.Add(new FolderItem(name, dir, songs));
            }
            foreach (var x in list.OrderBy(f => f.Directory.ToLower()))
            {
                collection.Add(x);
            }
            return collection;
        }

        public static ObservableCollection<GenreItem> GetGenreItems()
        {
            ObservableCollection<GenreItem> collection = new ObservableCollection<GenreItem>();
            var query = SongsConn().OrderBy(g => g.Genre).GroupBy(e => e.Genre).ToList();
            foreach (var item in query)
            {
                TimeSpan duration = TimeSpan.Zero;
                int songs = 0;
                foreach (var song in item)
                {
                    duration += song.Duration;
                    songs++;
                }
                collection.Add(new GenreItem(duration, item.FirstOrDefault().Genre, songs));
            }
            return collection;
        }

        public async static Task<ObservableCollection<GenreItem>> GetGenreItemsAsync()
        {
            ObservableCollection<GenreItem> collection = new ObservableCollection<GenreItem>();
            var query = await SongsConnAsync().OrderBy(g => g.Genre).ToListAsync();
            var result = query.GroupBy(x => x.Genre);
            foreach (var item in result)
            {
                TimeSpan duration = TimeSpan.Zero;
                int songs = 0;
                foreach (var song in item)
                {
                    duration += song.Duration;
                    songs++;
                }
                collection.Add(new GenreItem(duration, item.FirstOrDefault().Genre, songs));
            }
            return collection;
        }

        public static ObservableCollection<PlaylistItem> GetPlaylistItems()
        {
            ObservableCollection<PlaylistItem> collection = new ObservableCollection<PlaylistItem>();
            var query1 = ConnectionDb().Table<SmartPlaylistsTable>().OrderBy(p => p.SmartPlaylistId).ToList();
            Dictionary<int,string> ids = ApplicationSettingsHelper.PredefinedSmartPlaylistsId();
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string name;
            foreach (var item in query1)
            {
                if (ids.TryGetValue(item.SmartPlaylistId,out name))
                {
                    collection.Add(new PlaylistItem(item.SmartPlaylistId, true, loader.GetString(name)));
                }
                else
                {
                    //collection.Add(new PlaylistItem(item.SmartPlaylistId, true, item.Name));
                }
                
            }
            query1 = ConnectionDb().Table<SmartPlaylistsTable>().OrderBy(p => p.Name).ToList();
            foreach (var item in query1)
            {
                if (ids.TryGetValue(item.SmartPlaylistId, out name))
                {
                    //collection.Add(new PlaylistItem(item.SmartPlaylistId, true, loader.GetString(name)));
                }
                else
                {
                    collection.Add(new PlaylistItem(item.SmartPlaylistId, true, item.Name));
                }

            }


            var query = ConnectionDb().Table<PlainPlaylistsTable>().OrderBy(p => p.Name).ToList();
            foreach (var item in query)
            {
                collection.Add(new PlaylistItem(item.PlainPlaylistId, false, item.Name));
            }
            return collection;
        }

        public static ObservableCollection<SongItem> GetSongItems()
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();

            var query = SongsConn().OrderBy(s => s.Title);
            var result = query.ToList();
            foreach (var item in result)
            {
                songs.Add(CreateSongItem(item));
            }
            return songs;
        }

        public async static Task<ObservableCollection<SongItem>> GetSongItemsAsync()
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();

            var result = await SongsConnAsync().OrderBy(s => s.Title).ToListAsync();
            foreach (var item in result)
            {
                songs.Add(CreateSongItem(item));
            }
            return songs;
        }

        public static ObservableCollection<SongItem> GetSongItemsFromAlbum(string albumParam, string artistParam)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();

            if (artistParam == null)
            {
                var query = SongsConn().OrderBy(s => s.Track).Where(a => a.Album.Equals(albumParam)).ToList();
                foreach (var item in query)
                {
                    songs.Add(CreateSongItem(item));
                }
            }
            else
            {
                var query = SongsConn().OrderBy(s => s.Track).Where(w => w.Artists.Equals(artistParam)).Where(a => a.Album.Equals(albumParam)).ToList();
                foreach (var item in query)
                {
                    songs.Add(CreateSongItem(item));
                }
            }
            return songs;
        }

        public static ObservableCollection<SongItem> GetSongItemsFromFolder(string directory)
        {
            ObservableCollection<SongItem> folders = new ObservableCollection<SongItem>();
            var query = SongsConn().OrderBy(s => s.Title).ToList();
            foreach (var item in query)
            {
                if (Path.GetDirectoryName(item.Path).Equals(directory))
                {
                    folders.Add(CreateSongItem(item));
                }
            }
            return folders;
        }

        public async static Task<ObservableCollection<SongItem>> GetSongItemsFromFolderAsync(string directory)
        {
            ObservableCollection<SongItem> folders = new ObservableCollection<SongItem>();
            var query = await SongsConnAsync().OrderBy(s => s.Title).ToListAsync();
            foreach (var item in query)
            {
                if (Path.GetDirectoryName(item.Path).Equals(directory))
                {
                    folders.Add(CreateSongItem(item));
                }
            }
            return folders;
        }

        public static ObservableCollection<SongItem> GetSongItemsFromGenre(string genre)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var query = SongsConn().OrderBy(s => s.Title).Where(g => g.Genre.Equals(genre)).ToList();
            foreach (var item in query)
            {
                songs.Add(CreateSongItem(item));
            }
            return songs;
        }

        public async static Task<ObservableCollection<SongItem>> GetSongItemsFromGenreAsync(string genre)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var query = await SongsConnAsync().OrderBy(s => s.Title).Where(g => g.Genre.Equals(genre)).ToListAsync();
            foreach (var item in query)
            {
                songs.Add(CreateSongItem(item));
            }
            return songs;
        }

        public static ObservableCollection<SongItem> GetSongItemsFromArtist(string artistParam)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var query = SongsConn().OrderBy(s => s.Title).Where(a => a.Artists.Equals(artistParam)).ToList();
            foreach (var item in query)
            {
                songs.Add(CreateSongItem(item));
            }
            return songs;
        }

        public async static Task<ObservableCollection<SongItem>> GetSongItemsFromArtistAsync(string artistParam)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var query = await SongsConnAsync().OrderBy(s => s.Title).Where(a => a.Artists.Equals(artistParam)).ToListAsync();
            foreach (var item in query)
            {
                songs.Add(CreateSongItem(item));
            }
            return songs;
        }

        public static ObservableCollection<SongItem> GetSongItemsFromPlainPlaylist(int id)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            SQLiteConnection conn = ConnectionDb();
            var count = conn.Table<PlainPlaylistEntryTable>().Where(x => x.PlaylistId == id).Count();
            if (count != 0)
            {
                //var query = from e in conn.Table<PlainPlaylistEntryTable>()
                //            join s in conn.Table<SongsTable2>() on e.SongId equals s.SongId
                //            where e.PlaylistId.Equals(id)
                //            select s;
                List<SongsTable2> list = conn.Query<SongsTable2>("select * from PlainPlaylistEntryTable inner join SongsTable2 on PlainPlaylistEntryTable.SongId = SongsTable2.SongId where PlainPlaylistEntryTable.PlaylistId = ?  AND SongsTable2.IsAvailable = 1 order by PlainPlaylistEntryTable.Place", id);//available


                foreach (var item in list)
                {
                    songs.Add(CreateSongItem(item));
                }
            }
            
            return songs;
        }
        //ToDo
        public async static Task<ObservableCollection<SongItem>> GetSongItemsFromPlainPlaylistAsync(int id)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            SQLiteConnection conn = ConnectionDb();
            var query = from e in conn.Table<PlainPlaylistEntryTable>()
                        join s in conn.Table<SongsTable2>() on e.SongId equals s.SongId
                        where e.PlaylistId.Equals(id)
                        select s;
            foreach (var item in query)
            {
                songs.Add(CreateSongItem(item));
            }
            return songs;
        }

        public static ObservableCollection<SongItem> GetSongItemsFromSmartPlaylist(int id)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            SQLiteConnection conn = ConnectionDb();

            List<int> list = new List<int>();
            var q2 = conn.Table<SmartPlaylistsTable>().Where(p => p.SmartPlaylistId.Equals(id)).FirstOrDefault();
            string name = q2.Name;
            int maxNumber = q2.SongsNumber;
            string sorting = SPUtility.SPsorting[q2.SortBy];

            string less = null;
            string greater = null;
            bool ORcondition;

            var query = conn.Table<SmartPlaylistEntryTable>().Where(e => e.PlaylistId.Equals(id)).ToList();
            if (query.Count != 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("select * from SongsTable2 where IsAvailable > 0 AND ");

                foreach (var x in query) //x = condition
                {
                    ORcondition = false;
                    string comparison = SPUtility.SPConditionComparison[x.Comparison];
                    string item = SPUtility.SPConditionItem[x.Item];
                    string value = x.Value;

                    if (x.Comparison.Equals(SPUtility.Comparison.Contains))
                    {
                        value = "'%" + value + "%'";
                    }
                    else if (x.Comparison.Equals(SPUtility.Comparison.DoesNotContain))
                    {
                        value = "'%" + value + "%'";
                    }
                    else if (x.Comparison.Equals(SPUtility.Comparison.StartsWith))
                    {
                        value = "'%" + value + "'";
                    }
                    else if (x.Comparison.Equals(SPUtility.Comparison.EndsWith))
                    {
                        value = "'" + value + "%'";
                    }
                    else if (x.Comparison.Equals(SPUtility.Comparison.Is))
                    {
                        value = "'" + value + "'";
                    }
                    else if (x.Comparison.Equals(SPUtility.Comparison.IsNot))
                    {
                        value = "'" + value + "'";
                    }
                    else if (x.Comparison.Equals(SPUtility.Comparison.IsGreater))
                    {
                        value = "'" + value + "'";
                    }
                    else if (x.Comparison.Equals(SPUtility.Comparison.IsLess))
                    {
                        value = "'" + value + "'";
                    }
                    else if (x.Comparison.Equals(SPUtility.ComparisonEx.IsGreaterOR))
                    {
                        greater = "'" + value + "'";
                        ORcondition = true;
                    }
                    else if (x.Comparison.Equals(SPUtility.ComparisonEx.IsLessOR))
                    {
                        less = "'" + value + "'";
                        ORcondition = true;
                    }


                    if (greater != null && less != null)
                    {
                        builder.Append("(").Append(item).Append(" < ").Append(less).Append(" OR ").Append(item).Append(" > ").Append(greater).Append(") AND ");
                        greater = null;
                        less = null;
                    }
                    if (!ORcondition)
                    {
                        builder.Append(item).Append(" ").Append(comparison).Append(" ").Append(value).Append(" AND ");
                    }
                }
                builder.Remove(builder.Length - 4, 4);
                builder.Append("order by ").Append(sorting).Append(" limit ").Append(maxNumber);

                List<SongsTable2> q = conn.Query<SongsTable2>(builder.ToString());


                foreach (var x in q)
                {
                    songs.Add(CreateSongItem(x));
                }
            }
            
            return songs;
        }

        public async static Task<ObservableCollection<SongItem>> GetSongItemsFromSmartPlaylistAsync(int id)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            SQLiteAsyncConnection conn = AsyncConnectionDb();

            List<int> list = new List<int>();
            var q2 = await conn.Table<SmartPlaylistsTable>().Where(p => p.SmartPlaylistId.Equals(id)).FirstOrDefaultAsync();
            string name = q2.Name;
            int maxNumber = q2.SongsNumber;
            string sorting = SPUtility.SPsorting[q2.SortBy];

            string less = null;
            string greater = null;
            bool ORcondition;

            var query = await conn.Table<SmartPlaylistEntryTable>().Where(e => e.PlaylistId.Equals(id)).ToListAsync();
            if (query.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("select * from SongsTable2 where IsAvailable > 0 AND ");

                foreach (var x in query) //x = condition
                {
                    ORcondition = false;
                    string comparison = SPUtility.SPConditionComparison[x.Comparison];
                    string item = SPUtility.SPConditionItem[x.Item];
                    string value = x.Value;

                    if (x.Comparison.Equals(SPUtility.Comparison.Contains))
                    {
                        value = "'%" + value + "%'";
                    }
                    else if (x.Comparison.Equals(SPUtility.Comparison.DoesNotContain))
                    {
                        value = "'%" + value + "%'";
                    }
                    else if (x.Comparison.Equals(SPUtility.Comparison.StartsWith))
                    {
                        value = "'" + value + "%'";
                    }
                    else if (x.Comparison.Equals(SPUtility.Comparison.EndsWith))
                    {
                        value = "'%" + value + "'";
                    }
                    else if (x.Comparison.Equals(SPUtility.Comparison.Is))
                    {
                        value = "'" + value + "'";
                    }
                    else if (x.Comparison.Equals(SPUtility.Comparison.IsNot))
                    {
                        value = "'" + value + "'";
                    }
                    else if (x.Comparison.Equals(SPUtility.Comparison.IsGreater))
                    {
                        value = "'" + value + "'";
                    }
                    else if (x.Comparison.Equals(SPUtility.Comparison.IsLess))
                    {
                        value = "'" + value + "'";
                    }
                    else if (x.Comparison.Equals(SPUtility.ComparisonEx.IsGreaterOR))
                    {
                        greater = "'" + value + "'";
                        ORcondition = true;
                    }
                    else if (x.Comparison.Equals(SPUtility.ComparisonEx.IsLessOR))
                    {
                        less = "'" + value + "'";
                        ORcondition = true;
                    }


                    if (greater != null && less != null)
                    {
                        builder.Append("(").Append(item).Append(" < ").Append(less).Append(" OR ").Append(item).Append(" > ").Append(greater).Append(") AND ");
                        greater = null;
                        less = null;
                    }
                    if (!ORcondition)
                    {
                        builder.Append(item).Append(" ").Append(comparison).Append(" ").Append(value).Append(" AND ");
                    }
                }
                builder.Remove(builder.Length - 4, 4);
                builder.Append("order by ").Append(sorting).Append(" limit ").Append(maxNumber);

                List<SongsTable2> q = await conn.QueryAsync<SongsTable2>(builder.ToString());


                foreach (var x in q)
                {
                    songs.Add(CreateSongItem(x));
                }

            }
            
            return songs;
        }


        public async static Task<string> GetLyrics(int id)
        {
            var result = await SongsConnAsync().Where(s => s.SongId.Equals(id)).ToListAsync();
            return result.FirstOrDefault().Lyrics;
        }

        #endregion


        #region Select

        public static SongData SelectSongData(int _songId)
        {
            var q = SongsConn().Where(e => e.SongId.Equals(_songId)).FirstOrDefault();
            SongData s = CreateSongData(q);
            return s;
        }

        public static NowPlayingSong SelectSongFromNowPlaying(int songId)
        {
            var result = ConnectionDb().Table<NowPlayingTable>().Where(x => x.SongId.Equals(songId)).FirstOrDefault();
            return CreateNowPlayingSong(result);
        }

        public static NowPlayingSong SelectSongFromNowPlayingByPosition(int position)
        {
            var result = ConnectionDb().Table<NowPlayingTable>().Where(x => x.Position.Equals(position)).FirstOrDefault();
            return CreateNowPlayingSong(result);
        }

        public static List<NowPlayingSong> SelectAllSongsFromNowPlaying()
        {
            var query = ConnectionDb().Table<NowPlayingTable>().OrderBy(s => s.Position).ToList();
            List<NowPlayingSong> list = new List<NowPlayingSong>();
            foreach (var e in query)
            {
                list.Add(CreateNowPlayingSong(e));
            }
            return list;
        }

        public static ObservableCollection<SongItem> SelectAllSongItemsFromNowPlaying()
        {
            var conn = ConnectionDb();
            var query = conn.Table<NowPlayingTable>().OrderBy(e => e.Position).ToList();
            ObservableCollection<SongItem> list = new ObservableCollection<SongItem>();
            var conn2 = SongsConn();
            foreach (var e in query)
            {
                var query2 = conn2.Where(x => x.SongId.Equals(e.SongId)).FirstOrDefault();
                if (query2 != null)
                {
                    list.Add(CreateSongItem(query2));
                }
            }
            return list;
        }

        public async static Task<ObservableCollection<SongItem>> SelectAllSongItemsFromNowPlayingAsync()
        {
            var query = await AsyncConnectionDb().Table<NowPlayingTable>().OrderBy(e => e.Position).ToListAsync();
            ObservableCollection<SongItem> list = new ObservableCollection<SongItem>();
            foreach (var e in query)
            {
                var query2 = await SongsConnAsync().Where(x => x.SongId.Equals(e.SongId)).FirstOrDefaultAsync();
                list.Add(CreateSongItem(query2));
            }
            return list;
        }

        public static List<PlaylistItem> SelectPlainPlaylists()
        {
            var query = ConnectionDb().Table<PlainPlaylistsTable>().OrderBy(p=>p.Name).ToList();
            List<PlaylistItem> list = new List<PlaylistItem>();
            foreach (var item in query)
            {
                list.Add(new PlaylistItem(item.PlainPlaylistId, false, item.Name));
            }
            return list;
        }

        #endregion

        #region Update

        public async static Task UpdateSongData(SongData songData, int id)
        {
            var song = CreateSongsTable(songData);
            await AsyncConnectionDb().UpdateAsync(song);
        }

        public static void FillSongData(SongData song)
        {
            ConnectionDb().Execute("UPDATE SongsTable2 SET Artists = ?, Comment = ?, Composers = ?, Conductor = ?, Disc = ?, DiscCount = ?, FirstArtist = ?, FirstComposer = ?, TrackCount = ?,  WHERE SongId = ?", song.Tag.Artists, song.Tag.Comment, song.Tag.Composers, song.Tag.Conductor, song.Tag.Disc, song.Tag.DiscCount, song.Tag.FirstArtist, song.Tag.FirstComposer, song.Tag.TrackCount, song.SongId);
        }

        public async static Task UpdateSongStatistics(int id)
        {
            var conn = AsyncConnectionDb();
            var result = await conn.Table<SongsTable2>().Where(z => z.SongId.Equals(id)).FirstOrDefaultAsync();
            uint count = result.PlayCount + 1;
            int i = await conn.ExecuteAsync("UPDATE SongsTable2 SET LastPlayed = ?, PlayCount = ? WHERE SongId = ?", DateTime.Now, count, id);
        }

        public async static Task UpdateSongRating(int songId, int rating)
        {
            await AsyncConnectionDb().ExecuteAsync("UPDATE SongsTable2 SET Rating = ? WHERE SongId = ?", rating, songId);
        }

        public async static Task UpdateLyricsAsync(int id, string lyrics)
        {
            await AsyncConnectionDb().ExecuteAsync("UPDATE SongsTable2 SET Lyrics = ? WHERE SongId = ?", lyrics, id);
        }

        #endregion

        #region Search

        public static async Task<ObservableCollection<SongItem>> SearchSongs(string value)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var result = await SongsConnAsync().Where(t => (t.Title.Contains(value))).ToListAsync();
            foreach (var item in result)
            {
                songs.Add(CreateSongItem(item));
            }
            return songs;
        }

        public static ObservableCollection<AlbumItem> SearchAlbums(string value)
        {
            ObservableCollection<AlbumItem> collection = new ObservableCollection<AlbumItem>();
            var query = SongsConn().Where(s=>s.Album.Contains(value)).OrderBy(a => a.Album).GroupBy(g => g.Album).ToList();
            foreach (var item in query)
            {
                TimeSpan duration = TimeSpan.Zero;
                int songs = 0;
                string albumArtist = "";
                foreach (var song in item)
                {
                    if (song.AlbumArtist != "") albumArtist = song.AlbumArtist;
                    duration += song.Duration;
                    songs++;
                }
                collection.Add(new AlbumItem(item.FirstOrDefault().Album, null, albumArtist, duration, songs));
            }
            return collection;
        }

        public static  ObservableCollection<ArtistItem> SearchArtists(string value)
        {
            ObservableCollection<ArtistItem> collection = new ObservableCollection<ArtistItem>();
            var query = SongsConn().Where(s=>s.Artists.Contains(value) || s.AlbumArtist.Contains(value)).OrderBy(s => s.Artists).GroupBy(s => s.Artists).ToList();
            foreach (var artist in query)
            {
                ArtistItem artistItem = new ArtistItem();
                string name = artist.FirstOrDefault().Artists;
                int songs = 0;
                int albums = 0;
                TimeSpan duration = TimeSpan.Zero;
                foreach (var item in artist.GroupBy(e => e.Album).ToList())
                {
                    foreach (var song in item)
                    {
                        duration += song.Duration;
                        songs++;
                    }
                    albums++;
                }
                collection.Add(new ArtistItem(albums, name, duration, songs));
            }
            return collection;
        }

        #endregion

        #region Other

        public static int IsSongInDB(string path)
        {
            //long s = (long )size;
            var result = ConnectionDb().Table<SongsTable2>().Where(e => e.Path.Equals(path)).FirstOrDefault();

            if (result == null) return -1;
            else return 0;
            //else if (result.FileSize.Equals(s)) return 0;
            //else return result.SongId;

            //nieistnieje => -1
            //istnieje => 0
            //istnieje o innym rozmiarze => id
        }

        public static Dictionary<string, Tuple<int,int>> GetFilePaths()
        {
            Dictionary<string, Tuple<int,int>> dict = new Dictionary<string, Tuple<int,int>>();
            var result = ConnectionDb().Table<SongsTable2>().ToList();
            foreach (var x in result)
            {
                dict.Add(x.Path, new Tuple<int,int>(x.IsAvailable,x.SongId));
            }
            return dict;
        }

        public static void ChangeAvailability(IEnumerable<int> toAvailable)
        {
            var conn = ConnectionDb();
            conn.Execute("UPDATE SongsTable2 SET IsAvailable = 0");
            foreach (int id in toAvailable)
            {
                conn.Execute("UPDATE SongsTable2 SET IsAvailable = 1 WHERE SongId = ?", id);
            }
        }

        private static SongData CreateSongData(SongsTable2 q)
        {
            Tags tag = new Tags()
            {
                Album = q.Album,
                AlbumArtist = q.AlbumArtist,
                Artists = q.Artists,
                Comment = q.Comment,
                Composers = q.Composers,
                Conductor = q.Conductor,
                Disc = q.Disc,
                DiscCount = q.DiscCount,
                FirstArtist = q.FirstArtist,
                FirstComposer = q.FirstComposer,
                Genre = q.Genre,
                Lyrics = q.Lyrics,
                Rating = q.Rating,
                Title = q.Title,
                Track = q.Track,
                TrackCount = q.TrackCount,
                Year = q.Year
            };
            SongData s = new SongData()
            {
                Bitrate = q.Bitrate,
                DateAdded = q.DateAdded,
                Duration = q.Duration,
                Filename = q.Filename,
                FileSize = (ulong)q.FileSize,
                IsAvailable = q.IsAvailable,
                LastPlayed = q.LastPlayed,
                Path = q.Path,
                PlayCount = q.PlayCount,
                SongId = q.SongId,
                Tag = tag
            };
            return s;
        }

        private static SongsTable2 CreateSongsTable(SongData song)
        {
            return new SongsTable2()
            {
                Album = song.Tag.Album,
                AlbumArtist = song.Tag.AlbumArtist,
                Artists = song.Tag.Artists,
                Bitrate = song.Bitrate,
                Comment = song.Tag.Comment,
                Composers = song.Tag.Composers,
                Conductor = song.Tag.Conductor,
                DateAdded = song.DateAdded,
                Duration = song.Duration,
                Disc = song.Tag.Disc,
                DiscCount = song.Tag.DiscCount,
                Filename = song.Filename,
                FileSize = (long)song.FileSize,
                FirstArtist = song.Tag.FirstArtist,
                FirstComposer = song.Tag.FirstComposer,
                Genre = song.Tag.Genre,
                IsAvailable = song.IsAvailable,
                LastPlayed = song.LastPlayed,
                Lyrics = song.Tag.Lyrics,
                Path = song.Path,
                PlayCount = song.PlayCount,
                Rating = song.Tag.Rating,
                SongId = song.SongId,
                Title = song.Tag.Title,
                Track = song.Tag.Track,
                TrackCount = song.Tag.TrackCount,
                Year = song.Tag.Year
            };
        }

        private static NowPlayingSong CreateNowPlayingSong(NowPlayingTable npSong)
        {
            NowPlayingSong s = new NowPlayingSong();
            s.Artist = npSong.Artist;
            s.Path = npSong.Path;
            s.Position = npSong.Position;
            s.SongId = npSong.SongId;
            s.Title = npSong.Title;
            return s;
        }

        private static SongItem CreateSongItem(SongsTable2 q)
        {
            return new SongItem(q.Album, q.Artists, q.Composers, q.Duration, q.Path, (int)q.Rating, q.SongId, q.Title,(int) q.Track, q.Year);
        }

        public static FileInfo GetFileInfo(int songId)
        {
            var q = SongsConn().Where(s => s.SongId.Equals(songId)).FirstOrDefault();
            FileInfo info = new FileInfo()
            {
                Album = q.Album,
                Artist = q.Artists,
                AlbumArtist = q.AlbumArtist,
                Bitrate = (int)q.Bitrate,
                Composer = q.Composers,
                DateAdded = q.DateAdded,
                FilePath =q.Path,
                FileSize = (int)q.FileSize,
                LastTimePlayed = q.LastPlayed,
                PlayCount = (int)q.PlayCount,
                Rating = (int)q.Rating,
                Title = q.Title,
                TrackNumber = (int)q.Track,
                Year = (int)q.Year,
            };
            return info;
        }

        #endregion


        #region Old

        public async static Task<List<int>> SelectAllSongsId()
        {
            List<int> list = new List<int>();

            var query = SongsConnAsync().OrderBy(s => s.SongId);
            var result = await query.ToListAsync();

            foreach (var x in result)
            {
                list.Add(x.SongId);
            }

            return list;
        }

        

        //public async static Task<Dictionary<int, SongData>> SelectAllSongsComplete()
        //{
        //    Dictionary<int, SongData> dict = new Dictionary<int, SongData>();

        //    var query = AsyncConnectionDb().Table<SongsTable>().OrderBy(s => s.SongId);
        //    var result = await query.ToListAsync();
        //    foreach (var x in result)
        //    {
        //        dict.Add(x.SongId, CreateSongData(x));
        //    }
        //    return dict;
        //}


        //public static ObservableCollection<ArtistItem> SelectAllArtists()
        //{
        //    ObservableCollection<ArtistItem> collection = new ObservableCollection<ArtistItem>();
        //    var query = ConnectionDb().Table<SongsTable>().OrderBy(s => s.Artist).GroupBy(s => s.Artist).ToList();
        //    int artistId = 0;
        //    foreach (var q in query)
        //    {
        //        ArtistItem artistItem = new ArtistItem();
        //        artistItem.ArtistName = q.FirstOrDefault().Artist;
        //        artistItem.Id = artistId;
        //        artistId++;
        //        int albumId = 0;
        //        foreach (var album in q.GroupBy(e => e.Album).ToList())
        //        {
        //            artistItem.AlbumsNumber++;
        //            AlbumItem albumItem= new AlbumItem();
        //            albumItem.AlbumName = album.FirstOrDefault().Album;
        //            albumItem.ArtistName = album.FirstOrDefault().Artist;
        //            albumItem.Id = albumId;
        //            albumId++;
        //            foreach (var song in album)
        //            {
        //                albumItem.SongsList.Add(CreateSongItem(song));
        //                albumItem.SongsNumber++;
        //                albumItem.TotalTime += song.Duration;
        //            }
        //            artistItem.AlbumsList.Add(albumItem);
        //        }
        //        collection.Add(artistItem);
        //    }
        //    return collection;
        //}

        //public static ObservableCollection<AlbumItem> SelectAllAlbums()
        //{
        //    ObservableCollection<AlbumItem> collection = new ObservableCollection<AlbumItem>();
        //    var query = ConnectionDb().Table<SongsTable>().OrderBy(s => s.Album).GroupBy(s => s.Album).ToList();
        //    int id = 0;
        //    foreach (var q in query)
        //    {
        //        AlbumItem albumItem = new AlbumItem();
        //        albumItem.AlbumName = q.FirstOrDefault().Album;
        //        albumItem.ArtistName = q.FirstOrDefault().Artist;
        //        albumItem.Id = id;
        //        id++;
        //        foreach (var l in q.ToList())
        //        {
        //            albumItem.SongsList.Add(CreateSongItem(l));
        //            albumItem.SongsNumber++;
        //            albumItem.TotalTime += l.Duration;
        //        }
        //        collection.Add(albumItem);
        //    }
        //    return collection;
        //}

        //public async static Task<ObservableCollection<AlbumItem>> SelectAllAlbumsAsync()
        //{
        //    ObservableCollection<AlbumItem> collection = new ObservableCollection<AlbumItem>();

        //    var query = AsyncConnectionDb().Table<SongsTable>().OrderBy(s => s.Album);
        //    var result = await query.ToListAsync();

        //    string albumName = "";
        //    bool first = true;
        //    AlbumItem aElement = new AlbumItem();
        //    foreach (var song in result)
        //    {
        //        SongItem songItem = new SongItem();
        //        songItem.Album = song.Album;
        //        songItem.Artist = song.Artist;
        //        songItem.Duration = song.Duration;
        //        songItem.SongId = song.SongId;
        //        songItem.Title = song.Title;

        //        if (song.Album.Equals(albumName))
        //        {

        //        }
        //        else
        //        {
        //            if (!first) collection.Add(aElement);
        //            else first = false;

        //            albumName = song.Album;

        //            aElement = new AlbumItem();
        //            aElement.ArtistName = song.Artist;
        //            aElement.AlbumName = song.Album;
        //        }
        //        aElement.SongsList.Add(songItem);
        //        aElement.SongsNumber += 1;
        //        aElement.TotalTime.Add(songItem.Duration);
        //    }

        //    collection.Add(aElement);

        //    return collection;
        //}




        //public async static Task<List<Playlist>> SelectAllPlainPlaylists()
        //{
        //    List<Playlist> playlistList = new List<Playlist>();

        //    var query = AsyncConnectionDb().Table<PlainPlaylistsTable>().OrderBy(p => p.Name);
        //    var result = await query.ToListAsync();

        //    foreach (var x in result)
        //    {
        //        Playlist p = await SelectPlainPlaylist(x.PlainPlaylistId);
        //        playlistList.Add(p);
        //    }

        //    return playlistList;
        //}

        //public async static Task<Playlist> SelectPlainPlaylist(int _playlistId)
        //{
        //    var query = AsyncConnectionDb().Table<PlainPlaylistEntryTable>().Where(e => e.PlaylistId.Equals(_playlistId)).OrderBy(e => e.Place);
        //    var result = await query.ToListAsync();
        //    List<int> list = new List<int>();
        //    foreach (var x in result)
        //    {
        //        list.Add(x.SongId);
        //    }
        //    var q2 = await AsyncConnectionDb().Table<PlainPlaylistsTable>().Where(p => p.PlainPlaylistId.Equals(_playlistId)).FirstOrDefaultAsync();
        //    string name = q2.Name;
        //    Playlist playlist = new Playlist();
        //    playlist.Name = name;
        //    playlist.IsSmartPlaylist = false;
        //    playlist.SongsIdList = list;

        //    return playlist;
        //}

        //public async static Task<List<Playlist>> SelectAllSmartPlaylist()
        //{
        //    List<Playlist> playlistList = new List<Playlist>();

        //    var query = AsyncConnectionDb().Table<SmartPlaylistsTable>().OrderBy(p => p.Name);
        //    var result = await query.ToListAsync();

        //    foreach (var x in result)
        //    {
        //        Playlist p = await SelectSmartPlaylist(x.SmartPlaylistId);
        //        playlistList.Add(p);
        //    }

        //    return playlistList;
        //}

        //public async static Task<Playlist> SelectSmartPlaylist(int _playlistId)
        //{
        //    List<int> list = new List<int>();
        //    var q2 = await AsyncConnectionDb().Table<SmartPlaylistsTable>().Where(p => p.SmartPlaylistId.Equals(_playlistId)).FirstOrDefaultAsync();
        //    string name = q2.Name;
        //    int maxNumber = q2.SongsNumber;
        //    string sorting = SPUtility.SPsorting[q2.SortBy];

        //    var query = AsyncConnectionDb().Table<SmartPlaylistEntryTable>().Where(e => e.PlaylistId.Equals(_playlistId));
        //    var result = await query.ToListAsync();

        //    StringBuilder builder = new StringBuilder();
        //    builder.Append("select SongId from SongsTable where ");


        //    foreach (var x in result)
        //    {
        //        string comparison = SPUtility.SPConditionComparison[x.Comparison];
        //        string item = SPUtility.SPConditionItem[x.Item];
        //        string value = x.Value;

        //        if (x.Comparison.Equals("Contains"))
        //        {
        //            value = "%" + value + "%";
        //        }
        //        else if (x.Comparison.Equals("DoesNotContain"))
        //        {
        //            value = "%" + value + "%";
        //        }
        //        else if (x.Comparison.Equals("StartsWith"))
        //        {
        //            value = "%" + value;
        //        }
        //        else if (x.Comparison.Equals("EndsWith"))
        //        {
        //            value = value + "%";
        //        }

        //        builder.Append(item).Append(" ").Append(comparison).Append(" ").Append(value).Append(" AND");
        //    }
        //    builder.Remove(builder.Length - 3, 3);
        //    builder.Append("orderby ").Append(sorting).Append(" limit ").Append(maxNumber);

        //    List<int> l1 = await AsyncConnectionDb().QueryAsync<int>(builder.ToString());

        //    Playlist playlist = new Playlist();
        //    playlist.Name = name;
        //    playlist.IsSmartPlaylist = true;
        //    playlist.SongsIdList = list;
        //    return playlist;
        //}

        
        #endregion
    }
}
