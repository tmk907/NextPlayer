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
            return conn;
        }

        public async static Task DeleteDatabase()
        {
            await AsyncConnectionDb().DropTableAsync<SongsTable>();
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
            await AsyncConnectionDb().CreateTableAsync<SongsTable>();
            await AsyncConnectionDb().CreateTableAsync<NowPlayingTable>();
        }

        public static void ResetSongsTable()
        {
            ConnectionDb().DropTable<SongsTable>();
            ConnectionDb().CreateTable<SongsTable>();
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

        public async static Task<int> InsertSong(SongData _song)
        {
            var newSong = new SongsTable()
            {
                Album = _song.Album,
                AlbumArtist = _song.AlbumArtist,
                Artist = _song.Artist,
                Bitrate = _song.Bitrate,
                DateAdded = _song.DateAdded,
                Duration = _song.Duration,
                Filename = _song.Filename,
                FileSize = (long) _song.FileSize,
                Genre = _song.Genre,
                LastPlayed = _song.LastPlayed,
                Lyrics = _song.Lyrics,
                Path = _song.Path,
                PlayCount = _song.PlayCount,
                Publisher = _song.Publisher,
                Rating = _song.Rating,
                Subtitle = _song.Subtitle,
                Title = _song.Title,
                TrackNumber = _song.TrackNumber,
                Year = _song.Year,
            };

            await AsyncConnectionDb().InsertAsync(newSong);
            return newSong.SongId;
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

        public async static Task AddGenreToPlaylistAsync(string genre, int playlistId)
        {
            var conn = ConnectionDb();
            var query = await AsyncConnectionDb().Table<SongsTable>().Where(s => s.Genre.Equals(genre)).ToListAsync();
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

        public async static Task AddArtistToPlaylistAsync(string artist, int playlistId)
        {
            var conn = ConnectionDb();
            var query = await AsyncConnectionDb().Table<SongsTable>().Where(s => s.Artist.Equals(artist)).ToListAsync();
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

        public async static Task AddAlbumToPlaylistAsync(string album, int playlistId)
        {
            var conn = ConnectionDb();
            var query = await AsyncConnectionDb().Table<SongsTable>().Where(s => s.Album.Equals(album)).ToListAsync();
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
            //var entry = ConnectionDb().Table<PlainPlaylistEntryTable>().Where(e => e.SongId.Equals(_songId)).FirstOrDefault();
            ConnectionDb().Delete<PlainPlaylistEntryTable>(primaryId);
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
            var song = await AsyncConnectionDb().Table<SongsTable>().Where(s => s.SongId.Equals(_songId)).FirstOrDefaultAsync();
            await AsyncConnectionDb().DeleteAsync(song);
        }

        public static void ClearNowPlaying()
        {
            ConnectionDb().DeleteAll<NowPlayingTable>();
        }

        #endregion

        #region Get

        public static ObservableCollection<ArtistItem> GetArtistItems()
        {
            ObservableCollection<ArtistItem> collection = new ObservableCollection<ArtistItem>();
            var query = ConnectionDb().Table<SongsTable>().OrderBy(s => s.Artist).GroupBy(s => s.Artist).ToList();
            foreach (var artist in query)
            {
                ArtistItem artistItem = new ArtistItem();
                string name = artist.FirstOrDefault().Artist;
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
            var query = await AsyncConnectionDb().Table<SongsTable>().OrderBy(s => s.Artist).ToListAsync();
            ObservableCollection<ArtistItem> collection = new ObservableCollection<ArtistItem>();
            var result = query.GroupBy(s => s.Artist);
            foreach (var artist in result)
            {
                ArtistItem artistItem = new ArtistItem();
                string name = artist.FirstOrDefault().Artist;
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
            var query = ConnectionDb().Table<SongsTable>().OrderBy(a => a.Album).GroupBy(g => g.Album).ToList();
            foreach (var item in query)
            {
                TimeSpan duration = TimeSpan.Zero;
                int songs = 0;
                string albumArtist = item.FirstOrDefault().Artist;
                foreach (var song in item)
                {
                    duration += song.Duration;
                    songs++;
                    //if (song.AlbumArtist != "") albumArtist = song.AlbumArtist;
                }
                if (item.FirstOrDefault().Album.Equals("Unknown")) albumArtist = "Various Artists";
                collection.Add(new AlbumItem(item.FirstOrDefault().Album, albumArtist, duration, songs));
            }
            return collection;
        }

        public static List<AlbumItem> GetAlbumItems(string artist)
        {
            List<AlbumItem> collection = new List<AlbumItem>();
            var query = ConnectionDb().Table<SongsTable>().Where(a => a.Artist.Equals(artist)).OrderBy(o => o.Album).GroupBy(g => g.Album).ToList();
            foreach (var item in query)
            {
                TimeSpan duration = TimeSpan.Zero;
                int songs = 0;
                foreach (var song in item)
                {
                    duration += song.Duration;
                    songs++;
                }
                collection.Add(new AlbumItem(item.FirstOrDefault().Album, item.FirstOrDefault().Artist, duration, songs));
            }
            return collection;
        }

        public static AlbumItem GetAlbumItem(string album, string artist)
        {
            List<SongsTable> query;
            if (artist == null ||  artist.Equals(""))
            {
                query = ConnectionDb().Table<SongsTable>().Where(a => a.Album.Equals(album)).ToList();
            }
            else
            {
                query = ConnectionDb().Table<SongsTable>().Where(a => a.Album.Equals(album)).Where(b => b.Artist.Equals(artist)).ToList();
            }
            
            TimeSpan duration = TimeSpan.Zero;
            int songs = 0;
            string albumArtist = query.FirstOrDefault().Artist;
            foreach (var item in query)
            {
                songs++;
                duration += item.Duration;
            }
            if (album.Equals("Unknown") && artist.Equals("")) albumArtist = "Various Artists";
            AlbumItem albumItem = new AlbumItem(query.FirstOrDefault().Album, albumArtist, duration, songs);
            return albumItem;
        }

        public static ObservableCollection<GenreItem> GetGenreItems()
        {
            ObservableCollection<GenreItem> collection = new ObservableCollection<GenreItem>();
            var query = ConnectionDb().Table<SongsTable>().OrderBy(g => g.Genre).GroupBy(e => e.Genre).ToList();
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
            var query = await AsyncConnectionDb().Table<SongsTable>().OrderBy(g => g.Genre).ToListAsync();
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

            var query = ConnectionDb().Table<SongsTable>().OrderBy(s => s.Title);
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

            var result = await AsyncConnectionDb().Table<SongsTable>().OrderBy(s => s.Title).ToListAsync();
            foreach (var item in result)
            {
                songs.Add(CreateSongItem(item));
            }
            return songs;
        }

        public static ObservableCollection<SongItem> GetSongItemsFromAlbum(string album, string artist)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();

            if (artist == null || artist.Equals(""))
            {
                var query = ConnectionDb().Table<SongsTable>().OrderBy(s => s.TrackNumber).Where(a => a.Album.Equals(album)).ToList();
                foreach (var item in query)
                {
                    songs.Add(CreateSongItem(item));
                }
            }
            else
            {
                var query = ConnectionDb().Table<SongsTable>().OrderBy(s => s.TrackNumber).Where(w => w.Artist.Equals(artist)).Where(a => a.Album.Equals(album)).ToList();
                foreach (var item in query)
                {
                    songs.Add(CreateSongItem(item));
                }
            }
            return songs;
        }

        public static ObservableCollection<SongItem> GetSongItemsFromGenre(string genre)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var query = ConnectionDb().Table<SongsTable>().OrderBy(s => s.Title).Where(g => g.Genre.Equals(genre)).ToList();
            foreach (var item in query)
            {
                songs.Add(CreateSongItem(item));
            }
            return songs;
        }

        public async static Task<ObservableCollection<SongItem>> GetSongItemsFromGenreAsync(string genre)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var query = await AsyncConnectionDb().Table<SongsTable>().OrderBy(s => s.Title).Where(g => g.Genre.Equals(genre)).ToListAsync();
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
                //            join s in conn.Table<SongsTable>() on e.SongId equals s.SongId
                //            where e.PlaylistId.Equals(id)
                //            select s;
                List<SongsTable> list = conn.Query<SongsTable>("select * from PlainPlaylistEntryTable inner join SongsTable on PlainPlaylistEntryTable.SongId = SongsTable.SongId order by PlainPlaylistEntryTable.Place");


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
                        join s in conn.Table<SongsTable>() on e.SongId equals s.SongId
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

            var query = conn.Table<SmartPlaylistEntryTable>().Where(e => e.PlaylistId.Equals(id)).ToList();
            if (query.Count != 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("select * from SongsTable where ");

                foreach (var x in query) //x = condition
                {
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

                    builder.Append(item).Append(" ").Append(comparison).Append(" ").Append(value).Append(" AND ");
                }
                builder.Remove(builder.Length - 4, 4);
                builder.Append("order by ").Append(sorting).Append(" limit ").Append(maxNumber);

                List<SongsTable> q = conn.Query<SongsTable>(builder.ToString());


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

            var query = await conn.Table<SmartPlaylistEntryTable>().Where(e => e.PlaylistId.Equals(id)).ToListAsync();
            if (query.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("select * from SongsTable where ");

                foreach (var x in query) //x = condition
                {
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

                    builder.Append(item).Append(" ").Append(comparison).Append(" ").Append(value).Append(" AND ");
                }
                builder.Remove(builder.Length - 4, 4);
                builder.Append("order by ").Append(sorting).Append(" limit ").Append(maxNumber);

                List<SongsTable> q = await conn.QueryAsync<SongsTable>(builder.ToString());


                foreach (var x in q)
                {
                    songs.Add(CreateSongItem(x));
                }

            }
            
            return songs;
        }

        #endregion


        #region Select

        public async static Task<SongData> SelectSongData(int _songId)
        {
            var q = await AsyncConnectionDb().Table<SongsTable>().Where(e => e.SongId.Equals(_songId)).FirstOrDefaultAsync();
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
            var query = ConnectionDb().Table<NowPlayingTable>().OrderBy(e => e.Position).ToList();
            ObservableCollection<SongItem> list = new ObservableCollection<SongItem>();
            foreach (var e in query)
            {
                var query2 = ConnectionDb().Table<SongsTable>().Where(x => x.SongId.Equals(e.SongId)).FirstOrDefault();
                list.Add(CreateSongItem(query2));
            }
            return list;
        }

        public async static Task<ObservableCollection<SongItem>> SelectAllSongItemsFromNowPlayingAsync()
        {
            var query = await AsyncConnectionDb().Table<NowPlayingTable>().OrderBy(e => e.Position).ToListAsync();
            ObservableCollection<SongItem> list = new ObservableCollection<SongItem>();
            foreach (var e in query)
            {
                var query2 = await AsyncConnectionDb().Table<SongsTable>().Where(x => x.SongId.Equals(e.SongId)).FirstOrDefaultAsync();
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

        //public async static Task<Playlist> SelectSmartPlaylistEntry()
        //{

        //}
        #endregion

        #region Update

        public async static Task UpdateSongData(SongData _song, int id)
        {
            var song = new SongsTable()
            {
                Album = _song.Album,
                AlbumArtist = _song.AlbumArtist,
                Artist = _song.Artist,
                Bitrate = _song.Bitrate,
                DateAdded = _song.DateAdded,
                Duration = _song.Duration,
                Filename = _song.Filename,
                FileSize = (long)_song.FileSize,
                Genre = _song.Genre,
                LastPlayed = _song.LastPlayed,
                Lyrics = _song.Lyrics,
                Path = _song.Path,
                PlayCount = _song.PlayCount,
                Publisher = _song.Publisher,
                Rating = _song.Rating,
                Subtitle = _song.Subtitle,
                Title = _song.Title,
                TrackNumber = _song.TrackNumber,
                Year = _song.Year,
                SongId = id,
            };
            await AsyncConnectionDb().UpdateAsync(song);
        }

        public async static Task UpdateSongStatistics(int id)
        {
            var conn = AsyncConnectionDb();
            var result = await conn.Table<SongsTable>().Where(z => z.SongId.Equals(id)).FirstOrDefaultAsync();
            uint count = result.PlayCount + 1;
            int i = await conn.ExecuteAsync("UPDATE SongsTable SET LastPlayed = ?, PlayCount = ? WHERE SongId = ?",DateTime.Now ,count, id);
        }

        private static void testUpdate(int id)
        {
            var a = ConnectionDb().Table<SongsTable>().Where(i => i.SongId.Equals(id)).FirstOrDefault();
        }

        #endregion

        #region Other

        public async static Task<int> IsSongInDB(string _path, ulong size)
        {
            long s = (long )size;
            var result = await AsyncConnectionDb().Table<SongsTable>().Where(e => e.Path.Equals(_path)).FirstOrDefaultAsync();

            if (result == null) return -1;
            else if (result.FileSize.Equals(s)) return 0;
            else return result.SongId;
            //nieistnieje => -1
            //istnieje => 0
            //istnieje o innym rozmiarze => id
        }

        private static SongData CreateSongData(SongsTable q)
        {
            SongData s = new SongData()
            {
                Album = q.Album,
                AlbumArtist = q.AlbumArtist,
                Artist = q.Artist,
                Bitrate = q.Bitrate,
                DateAdded = q.DateAdded,
                Duration = q.Duration,
                Filename = q.Filename,
                FileSize = (ulong)q.FileSize,
                Genre = q.Genre,
                LastPlayed = q.LastPlayed,
                Lyrics = q.Lyrics,
                Path = q.Path,
                PlayCount = q.PlayCount,
                Publisher = q.Publisher,
                Rating = q.Rating,
                SongId = q.SongId,
                Subtitle = q.Subtitle,
                Title = q.Title,
                TrackNumber = q.TrackNumber,
                Year = q.Year
            };
            return s;
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

        private static SongItem CreateSongItem(SongsTable q)
        {
            return new SongItem(q.Album, q.Artist, q.Duration, q.Path, q.SongId, q.Title,(int) q.TrackNumber);
        }

        #endregion


        #region Old

        public async static Task<List<int>> SelectAllSongsId()
        {
            List<int> list = new List<int>();

            var query = AsyncConnectionDb().Table<SongsTable>().OrderBy(s => s.SongId);
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
