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
    public class PlaylistViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private string genre;
        private string name;
        private bool isSmart;
        private int id;
        private bool isNowPlaying;

        public PlaylistViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        /// <summary>
        /// The <see cref="PageTitle" /> property's name.
        /// </summary>
        public const string PageTitlePropertyName = "PageTitle";

        private string pageTitle = "now playing";

        /// <summary>
        /// Sets and gets the PageTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string PageTitle
        {
            get
            {
                return pageTitle;
            }

            set
            {
                if (pageTitle == value)
                {
                    return;
                }

                pageTitle = value;
                RaisePropertyChanged(PageTitlePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Playlist" /> property's name.
        /// </summary>
        public const string PlaylistPropertyName = "Playlist";

        private ObservableCollection<SongItem> playlist = new ObservableCollection<SongItem>();

        /// <summary>
        /// Sets and gets the Playlist property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<SongItem> Playlist
        {
            get
            {
                if (IsInDesignMode)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        playlist.Add(new SongItem());
                    }
                }
                return playlist;
            }

            set
            {
                if (playlist == value)
                {
                    return;
                }

                playlist = value;
                RaisePropertyChanged(PlaylistPropertyName);
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
                        foreach(var song in Playlist)
                        {
                            if (song.SongId == item.SongId) break;
                            index++;
                        }
                        ApplicationSettingsHelper.SaveSongIndex(index);
                        Library.Current.SetNowPlayingList(Playlist);
                        if (NextPlayer.Common.SuspensionManager.SessionState.ContainsKey("nplist"))
                        {
                            NextPlayer.Common.SuspensionManager.SessionState.Remove("nplist");
                            navigationService.GoBack();
                        }
                        else
                        {
                            navigationService.NavigateTo(ViewNames.NowPlayingView, item.SongId);
                        }
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
                        
                        if (isNowPlaying)
                        {
                            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                            //Playlist = await DatabaseManager.SelectAllSongItemsFromNowPlaying();
                            LoadNowPlayingPlaylist();
                            PageTitle = loader.GetString("NowPlayingPlaylistPageTitle");
                        }
                        else
                        {
                            if (genre == null)
                            {
                                if (isSmart) 
                                {
                                    LoadSmartPlaylist();
                                }
                                else
                                {
                                    Playlist = DatabaseManager.GetSongItemsFromPlainPlaylist(id);
                                }
                                PageTitle = name.ToLower();
                            }
                            else
                            {
                                LoadGenrePlaylist();   
                                PageTitle = genre.ToLower();
                            }
                        }
                    }));
            }
        }
        private async void LoadNowPlayingPlaylist()
        {
            Playlist = Library.Current.NowPlayingList;
            //Playlist = await DatabaseManager.SelectAllSongItemsFromNowPlayingAsync();
        }
        private async void LoadGenrePlaylist()
        {
            Playlist = await DatabaseManager.GetSongItemsFromGenreAsync(genre);
        }
        private async void LoadSmartPlaylist()
        {
            Playlist = await DatabaseManager.GetSongItemsFromSmartPlaylistAsync(id);
        }
        private async void LoadPlainPlaylist()
        {
            Playlist = await DatabaseManager.GetSongItemsFromPlainPlaylistAsync(id);
        }
        public void Activate(object parameter, Dictionary<string, object> state)
        {
            Playlist.Clear();
            genre = null;
            isSmart = false;
            isNowPlaying = true;
            id = 0;
            name = "";
            if (parameter != null)
            {
                if (parameter.GetType() == typeof(string))
                {
                    String[] s = ParamConvert.ToStringArray(parameter as string);
                    if (s.Length >= 2 && s[0].Equals("genre")) genre = s[1];
                    else
                    {
                        if (s.Length >= 2 && s[0].Equals("smart")) isSmart = true;
                        if (s.Length >= 3 && (s[0].Equals("smart") || s[0].Equals("plain"))) 
                        {
                            id = Int32.Parse(s[1]);
                            name = s[2];
                        }
                    }
                    isNowPlaying = false;
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