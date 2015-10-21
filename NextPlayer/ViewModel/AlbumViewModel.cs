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
using Windows.UI;
using NextPlayerDataLayer.Constants;
using Windows.UI.Xaml.Controls;

namespace NextPlayer.ViewModel
{
    public class AlbumViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private string albumParam;
        private string artistParam;
        private int index;

        public AlbumViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            album = null;
            artist = null;
        }

        #region Properties
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
                if (IsInDesignMode) album = "nazwa albumu dluuuuuuuuuga aaaa dluuuuuuuuuga";
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
                if (IsInDesignMode) artist = "nazwa artysty dluuuuuuuuuga bbb dluuuuuuuuuga";
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

        /// <summary>
        /// The <see cref="BackgroundImage" /> property's name.
        /// </summary>
        public const string BackgroundImagePropertyName = "BackgroundImage";

        private BitmapImage backgroundImage = new BitmapImage();

        /// <summary>
        /// Sets and gets the BackgroundImage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public BitmapImage BackgroundImage
        {
            get
            {
                return backgroundImage;
            }

            set
            {
                if (backgroundImage == value)
                {
                    return;
                }

                backgroundImage = value;
                RaisePropertyChanged(BackgroundImagePropertyName);
            }
        }
        #endregion
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
                        index = SelectedIndex(item);
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
                        index = SelectedIndex(item);
                        navigationService.NavigateTo(ViewNames.FileInfoView, item.SongId);
                    }));
            }
        }

        private RelayCommand<SongItem> editTags;

        /// <summary>
        /// Gets the EditTags.
        /// </summary>
        public RelayCommand<SongItem> EditTags
        {
            get
            {
                return editTags
                    ?? (editTags = new RelayCommand<SongItem>(
                    item =>
                    {
                        index = SelectedIndex(item);
                        navigationService.NavigateTo(ViewNames.TagsEditor, item.SongId);
                    }));
            }
        }

        private RelayCommand<SongItem> share;

        /// <summary>
        /// Gets the Share.
        /// </summary>
        public RelayCommand<SongItem> Share
        {
            get
            {
                return share
                    ?? (share = new RelayCommand<SongItem>(
                    item =>
                    {
                        index = SelectedIndex(item);
                        String[] s = new String[2];
                        s[0] = "song";
                        s[1] = item.SongId.ToString();
                        navigationService.NavigateTo(ViewNames.BluetoothShare, ParamConvert.ToString(s));
                    }));
            }
        }


        private RelayCommand<object> scrollListView;

        /// <summary>
        /// Gets the ScrollListView.
        /// </summary>
        public RelayCommand<object> ScrollListView
        {
            get
            {
                return scrollListView
                    ?? (scrollListView = new RelayCommand<object>(
                    p =>
                    {
                        ListView l = (ListView)p;
                        
                        if (l.Items.Count > 0 && index > 0)
                        {
                            l.SelectedIndex = index;
                            l.UpdateLayout();
                            l.ScrollIntoView(l.SelectedItem, ScrollIntoViewAlignment.Leading);
                        }
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
                        Artist = a.AlbumArtist;
                        Duration = a.Duration;
                        Songs = DatabaseManager.GetSongItemsFromAlbum(albumParam, artistParam);
                        SetCover();
                    }));
            }
        }

        private async void SetCover()
        {
            
            bool isBGSet = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IsBGImageSet);
            bool isBGCover = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.ShowCoverAsBackground);
            BitmapImage originalCover = await Library.Current.GetOriginalCover(songs.FirstOrDefault().Path, false);
            if (isBGSet)
            {
                string path = ApplicationSettingsHelper.ReadSettingsValue(NextPlayerDataLayer.Constants.AppConstants.BackgroundImagePath) as string;
                if (path != null && path != "")
                {
                    BackgroundImage = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                }
                if (isBGCover)
                {
                    if (originalCover.PixelHeight > 0)
                    {
                        BackgroundImage = originalCover;
                    }
                }
            }
            else
            {
                BackgroundImage = new BitmapImage();
                if (isBGCover)
                {
                    if (originalCover.PixelHeight > 0)
                    {
                        BackgroundImage = originalCover;
                    }
                }
            }
            if (originalCover.PixelHeight > 0)
            {
                Cover = originalCover;
            }
            else
            {
                Cover = await Library.Current.GetDefaultCover(false);
            }
        }

        private int SelectedIndex(SongItem item)
        {
            int i = 0;
            foreach (var a in Songs)
            {
                if (a.SongId == item.SongId)
                {
                    break;
                }
                i++;
            }
            return i;
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {
            index = 0;
            albumParam = null;
            artistParam = null;
            Cover = new BitmapImage();
            if (state != null)
            {
                if (state.ContainsKey("index"))
                {
                    index = (int)state["index"];
                }
            }
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
            state["index"] = index;
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }
    }
}