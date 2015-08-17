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
using Windows.UI.Xaml.Media.Imaging;

namespace NextPlayer.ViewModel
{
    public class AlbumViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private string albumParam;
        private string artistParam;

        public AlbumViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            album = null;
            artist = null;
        }

        /// <summary>
        /// The <see cref="Album" /> property's name.
        /// </summary>
        public const string AlbumPropertyName = "Album";

        private string album = "";

        /// <summary>
        /// Sets and gets the Album property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Album
        {
            get
            {
                if (IsInDesignMode) album = "nazwa albumu dluuuuuuuuuga";
                return album;
            }

            set
            {
                if (album == value)
                {
                    return;
                }

                album = value;
                RaisePropertyChanged(AlbumPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Artist" /> property's name.
        /// </summary>
        public const string ArtistPropertyName = "Artist";

        private string artist = "";

        /// <summary>
        /// Sets and gets the Artist property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Artist
        {
            get
            {
                if (IsInDesignMode) artist = "nazwa artysty dluuuuuuuuuga";
                return artist;
            }

            set
            {
                if (artist == value)
                {
                    return;
                }

                artist = value;
                RaisePropertyChanged(ArtistPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Duration" /> property's name.
        /// </summary>
        public const string DurationPropertyName = "Duration";

        private TimeSpan duration = TimeSpan.Zero;

        /// <summary>
        /// Sets and gets the Duration property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                return duration;
            }

            set
            {
                if (duration == value)
                {
                    return;
                }

                duration = value;
                RaisePropertyChanged(DurationPropertyName);
            }
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
                if (songs.Count == 0)
                {
                    if (IsInDesignMode)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            songs.Add(new SongItem());
                        }
                    }
                }
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
        /// The <see cref="Cover" /> property's name.
        /// </summary>
        public const string CoverPropertyName = "Cover";

        private BitmapImage cover = new BitmapImage();

        /// <summary>
        /// Sets and gets the Cover property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public BitmapImage Cover
        {
            get
            {
                return cover;
            }

            set
            {
                if (cover == value)
                {
                    return;
                }

                cover = value;
                RaisePropertyChanged(CoverPropertyName);
            }
        }

        private RelayCommand<SongItem> addToNowPlaying;

        /// <summary>
        /// Gets the AddToNowPlaying.
        /// </summary>
        public RelayCommand<SongItem> AddToNowPlaying
        {
            get
            {
                return addToNowPlaying
                    ?? (addToNowPlaying = new RelayCommand<SongItem>(
                    item =>
                    {
                        Library.Current.AddToNowPlaying(item);
                    }));
            }
        }

        private RelayCommand<SongItem> addToPlaylist;

        /// <summary>
        /// Gets the AddToPlaylist.
        /// </summary>
        public RelayCommand<SongItem> AddToPlaylist
        {
            get
            {
                return addToPlaylist
                    ?? (addToPlaylist = new RelayCommand<SongItem>(
                    item =>
                    {
                        String[] s = new String[2];
                        s[0] = "song";
                        s[1] = item.SongId.ToString();
                        navigationService.NavigateTo(ViewNames.AddToPlaylistView, ParamConvert.ToString(s));
                    }));
            }
        }

        private RelayCommand<SongItem> showDetails;

        /// <summary>
        /// Gets the ShowDetails.
        /// </summary>
        public RelayCommand<SongItem> ShowDetails
        {
            get
            {
                return showDetails
                    ?? (showDetails = new RelayCommand<SongItem>(
                    item =>
                    {
                        navigationService.NavigateTo(ViewNames.FileInfoView, item.SongId);
                    }));
            }
        }

        private RelayCommand<SongItem> itemClicked;

        /// <summary>
        /// Gets the ItemClicked.
        /// </summary>
        public RelayCommand<SongItem> ItemClicked
        {
            get
            {
                return itemClicked
                    ?? (itemClicked = new RelayCommand<SongItem>(
                    item =>
                    {
                        int index = 0;
                        foreach (var song in songs)
                        {
                            if (song.SongId == item.SongId) break;
                            index++;
                        }
                        ApplicationSettingsHelper.SaveSongIndex(index);
                        Library.Current.SetNowPlayingList(Songs);
                        navigationService.NavigateTo(ViewNames.NowPlayingView, item.SongId);
                    }));
            }
        }

        private RelayCommand loadItems;

        /// <summary>
        /// Gets the LoadItems.
        /// </summary>
        public RelayCommand LoadItems
        {
            get
            {
                return loadItems
                    ?? (loadItems = new RelayCommand(
                    () =>
                    {
                        AlbumItem a = DatabaseManager.GetAlbumItem(albumParam, artistParam);
                        Album = a.Album;
                        Artist = a.Artist;
                        Duration = a.Duration;
                        Songs = DatabaseManager.GetSongItemsFromAlbum(albumParam, artistParam);
                        SetCover();
                    }));
            }
        }

        private async void SetCover()
        {
            Cover = await Library.Current.GetCover(songs.FirstOrDefault().Path);
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {
            albumParam = null;
            artistParam = null;
            if (parameter != null)
            {
                if (parameter.GetType() == typeof(string))
                {
                    String[] s = ParamConvert.ToStringArray(parameter as string);
                    if (s.Length >= 2 && s[0].Equals("album")) albumParam = s[1];
                    if (s.Length >= 4 && s[2].Equals("artist")) artistParam = s[3];
                }
            }
        }

        public void Deactivate(Dictionary<string, object> state)
        {
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }
    }
}