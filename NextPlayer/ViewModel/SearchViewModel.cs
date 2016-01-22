using NextPlayer.Constants;
using NextPlayerDataLayer.Model;
using NextPlayerDataLayer.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextPlayerDataLayer.Helpers;
using NextPlayer.Converters;

namespace NextPlayer.ViewModel
{
    public class SearchViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;

        public SearchViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        /// <summary>
        /// The <see cref="Songs" /> property's name.
        /// </summary>
        public const string SongsPropertyName = "Songs";

        private ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();

        /// <summary>
        /// Sets and gets the Songs property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<SongItem> Songs
        {
            get
            {
                return songs;
            }

            set
            {
                if (songs == value)
                {
                    return;
                }

                songs = value;
                RaisePropertyChanged(SongsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Albums" /> property's name.
        /// </summary>
        public const string AlbumsPropertyName = "Albums";

        private ObservableCollection<AlbumItem> albums = new ObservableCollection<AlbumItem>();

        /// <summary>
        /// Sets and gets the Albums property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<AlbumItem> Albums
        {
            get
            {
                return albums;
            }

            set
            {
                if (albums == value)
                {
                    return;
                }

                albums = value;
                RaisePropertyChanged(AlbumsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Artists" /> property's name.
        /// </summary>
        public const string ArtistsPropertyName = "Artists";

        private ObservableCollection<ArtistItem> artists = new ObservableCollection<ArtistItem>();

        /// <summary>
        /// Sets and gets the Artists property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ArtistItem> Artists
        {
            get
            {
                return artists;
            }

            set
            {
                if (artists == value)
                {
                    return;
                }

                artists = value;
                RaisePropertyChanged(ArtistsPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="SearchQuery" /> property's name.
        /// </summary>
        public const string SearchQueryPropertyName = "SearchQuery";

        private string searchQuery = "";

        /// <summary>
        /// Sets and gets the SearchQuery property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SearchQuery
        {
            get
            {
                return searchQuery;
            }

            set
            {
                if (searchQuery == value)
                {
                    return;
                }

                searchQuery = value;
                RaisePropertyChanged(SearchQueryPropertyName);
            }
        }

        private RelayCommand<SongItem> songClicked;

        /// <summary>
        /// Gets the SongClicked.
        /// </summary>
        public RelayCommand<SongItem> SongClicked
        {
            get
            {
                return songClicked
                    ?? (songClicked = new RelayCommand<SongItem>(
                    song =>
                    {
                        bool find = false;
                        int i = 0;
                        int index = 0;
                        List<SongItem> list = new List<SongItem>();
                        foreach (var s in Songs)
                        {
                            if (s.SongId == song.SongId)
                            {
                                find = true;
                                index = i;
                            }
                            i++;
                        }

                        ApplicationSettingsHelper.SaveSongIndex(index);
                        Library.Current.SetNowPlayingList(songs);

                        navigationService.NavigateTo(ViewNames.NowPlayingView, song.SongId);
                    }));
            }
        }

        private RelayCommand<AlbumItem> albumClicked;

        /// <summary>
        /// Gets the AlbumClicked.
        /// </summary>
        public RelayCommand<AlbumItem> AlbumClicked
        {
            get
            {
                return albumClicked
                    ?? (albumClicked = new RelayCommand<AlbumItem>(
                    album =>
                    {
                        String[] s = new String[4];
                        s[0] = "album";
                        s[1] = album.AlbumParam;
                        s[2] = "artist";
                        s[3] = album.ArtistParam;
                        navigationService.NavigateTo(ViewNames.AlbumView, ParamConvert.ToString(s));
                    }));
            }
        }

        private RelayCommand<ArtistItem> artistClicked;

        /// <summary>
        /// Gets the ArtistClicked.
        /// </summary>
        public RelayCommand<ArtistItem> ArtistClicked
        {
            get
            {
                return artistClicked
                    ?? (artistClicked = new RelayCommand<ArtistItem>(
                    artist =>
                    {
                        String[] s = new String[2];
                        s[0] = "artist";
                        s[1] = artist.ArtistParam;
                        navigationService.NavigateTo(ViewNames.AlbumsView, ParamConvert.ToString(s));
                    }));
            }
        }

        private RelayCommand searchClick;

        /// <summary>
        /// Gets the SearchClick.
        /// </summary>
        public RelayCommand SearchClick
        {
            get
            {
                return searchClick
                    ?? (searchClick = new RelayCommand(
                    () =>
                    {
                        Search(searchQuery);
                    }));
            }
        }

        private RelayCommand<SongItem> addSongToPlaylist;

        /// <summary>
        /// Gets the AddSongToPlaylist.
        /// </summary>
        public RelayCommand<SongItem> AddSongToPlaylist
        {
            get
            {
                return addSongToPlaylist
                    ?? (addSongToPlaylist = new RelayCommand<SongItem>(
                    item =>
                    {
                        String[] s = new String[2];
                        s[0] = "song";
                        s[1] = item.SongId.ToString();
                        navigationService.NavigateTo(ViewNames.AddToPlaylistView, ParamConvert.ToString(s));
                    }));
            }
        }

        private RelayCommand<ArtistItem> addArtistToPlaylist;

        /// <summary>
        /// Gets the AddArtistToPlaylist.
        /// </summary>
        public RelayCommand<ArtistItem> AddArtistToPlaylist
        {
            get
            {
                return addArtistToPlaylist
                    ?? (addArtistToPlaylist = new RelayCommand<ArtistItem>(
                    item =>
                    {
                        String[] s = new String[2];
                        s[0] = "artist";
                        s[1] = item.ArtistParam;
                        navigationService.NavigateTo(ViewNames.AddToPlaylistView, ParamConvert.ToString(s));
                    }));
            }
        }

        private RelayCommand<AlbumItem> addAlbumToPlaylist;

        /// <summary>
        /// Gets the AddAlbumToPlaylist.
        /// </summary>
        public RelayCommand<AlbumItem> AddAlbumToPlaylist
        {
            get
            {
                return addAlbumToPlaylist
                    ?? (addAlbumToPlaylist = new RelayCommand<AlbumItem>(
                    item =>
                    {
                        String[] s = new String[4];
                        s[0] = "album";
                        s[1] = item.AlbumParam;
                        s[2] = "artist";
                        s[3] = item.ArtistParam;
                        navigationService.NavigateTo(ViewNames.AddToPlaylistView, ParamConvert.ToString(s));
                    }));
            }
        }

        private RelayCommand<SongItem> addSongToNP;

        /// <summary>
        /// Gets the AddSongToNP.
        /// </summary>
        public RelayCommand<SongItem> AddSongToNP
        {
            get
            {
                return addSongToNP
                    ?? (addSongToNP = new RelayCommand<SongItem>(
                    item =>
                    {
                        Library.Current.AddToNowPlaying(item);
                    }));
            }
        }

        private RelayCommand<AlbumItem> playAlbumNow;

        /// <summary>
        /// Gets the PlayAlbumNow.
        /// </summary>
        public RelayCommand<AlbumItem> PlayAlbumNow
        {
            get
            {
                return playAlbumNow
                    ?? (playAlbumNow = new RelayCommand<AlbumItem>(
                    item =>
                    {
                        var g = DatabaseManager.GetSongItemsFromAlbum(item.AlbumParam, item.ArtistParam);
                        Library.Current.SetNowPlayingList(g);
                        ApplicationSettingsHelper.SaveSongIndex(0);
                        navigationService.NavigateTo(ViewNames.NowPlayingView, "start");
                    }));
            }
        }

        private RelayCommand<ArtistItem> playArtistNow;

        /// <summary>
        /// Gets the PlayArtistNow.
        /// </summary>
        public RelayCommand<ArtistItem> PlayArtistNow
        {
            get
            {
                return playArtistNow
                    ?? (playArtistNow = new RelayCommand<ArtistItem>(
                    item =>
                    {
                        var g = DatabaseManager.GetSongItemsFromArtist(item.ArtistParam);
                        g.OrderBy(s => s.Album).ThenBy(t => t.TrackNumber);
                        Library.Current.SetNowPlayingList(g);
                        ApplicationSettingsHelper.SaveSongIndex(0);
                        navigationService.NavigateTo(ViewNames.NowPlayingView, "start");
                    }));
            }
        }

        private RelayCommand<AlbumItem> addAlbumToNP;

        /// <summary>
        /// Gets the AddAlbumToNP.
        /// </summary>
        public RelayCommand<AlbumItem> AddAlbumToNP
        {
            get
            {
                return addAlbumToNP
                    ?? (addAlbumToNP = new RelayCommand<AlbumItem>(
                    item =>
                    {
                        AddAlbumToNowPlayingAsync(item);
                    }));
            }
        }
        public async void AddAlbumToNowPlayingAsync(AlbumItem item)
        {
            var g = DatabaseManager.GetSongItemsFromAlbum(item.AlbumParam, item.ArtistParam);
            Library.Current.AddToNowPlaying(g);
        }
        private RelayCommand<ArtistItem> addArtistToNP;

        /// <summary>
        /// Gets the AddArtistToNP.
        /// </summary>
        public RelayCommand<ArtistItem> AddArtistToNP
        {
            get
            {
                return addArtistToNP
                    ?? (addArtistToNP = new RelayCommand<ArtistItem>(
                    item =>
                    {
                        AddArtistToNowPlayingAsync(item);
                    }));
            }
        }
        public async void AddArtistToNowPlayingAsync(ArtistItem item)
        {
            var g = await DatabaseManager.GetSongItemsFromArtistAsync(item.Artist);
            g.OrderBy(s => s.Album).ThenBy(t => t.TrackNumber);
            Library.Current.AddToNowPlaying(g);
        }
        public async void Search(string value)
        {
            Songs = await DatabaseManager.SearchSongs(value);
            Albums = DatabaseManager.SearchAlbums(value);
            Artists = DatabaseManager.SearchArtists(value);
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {
        }

        public void Deactivate(Dictionary<string, object> state)
        {
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }
    }
}