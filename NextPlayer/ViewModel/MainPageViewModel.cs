using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using NextPlayer.Constants;
using NextPlayerDataLayer.Constants;
using NextPlayerDataLayer.Helpers;
using NextPlayerDataLayer.Model;
using NextPlayerDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Xaml.Media.Imaging;

namespace NextPlayer.ViewModel
{
    public class MainPageViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private bool IsMyBackgroundTaskRunning
        {
            get
            {
                object value = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.BackgroundTaskState);
                if (value == null)
                {
                    return false;
                }
                else
                {
                    bool a = ((String)value).Equals(AppConstants.BackgroundTaskRunning);
                    return a;
                }
            }
        }
        private int CurrentSongIndex
        {
            get
            {
                return ApplicationSettingsHelper.ReadSongIndex();
            }
            set
            {
                ApplicationSettingsHelper.SaveSongIndex((int)value);
            }
        }
        private AutoResetEvent SererInitialized;

        public MainPageViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            SererInitialized = new AutoResetEvent(false);

            App.Current.Suspending += ForegroundApp_Suspending;
            App.Current.Resuming += ForegroundApp_Resuming;

            
        }

        #region Properties
        /// <summary>
        /// The <see cref="Title" /> property's name.
        /// </summary>
        public const string TitlePropertyName = "Title";

        private string title = "-";

        /// <summary>
        /// Sets and gets the Title property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                if (title == value)
                {
                    return;
                }

                title = value;
                RaisePropertyChanged(TitlePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Artist" /> property's name.
        /// </summary>
        public const string ArtistPropertyName = "Artist";

        private string artist = "-";

        /// <summary>
        /// Sets and gets the Artist property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Artist
        {
            get
            {
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
        #endregion
        #region CommandsGoTo
        private RelayCommand goToNowPlayingListPage;

        /// <summary>
        /// Gets the GoToNowPlayingListPage.
        /// </summary>
        public RelayCommand GoToNowPlayingListPage
        {
            get
            {
                return goToNowPlayingListPage
                    ?? (goToNowPlayingListPage = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo(ViewNames.PlaylistView);
                    }));
            }
        }

        private RelayCommand goToSongsPage;

        /// <summary>
        /// Gets the GoToSongsPage.
        /// </summary>
        public RelayCommand GoToSongsPage
        {
            get
            {
                return goToSongsPage
                    ?? (goToSongsPage = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo(ViewNames.SongsView);
                    }));
            }
        }

        private RelayCommand goToAlbumsPage;

        /// <summary>
        /// Gets the GoToAlbumsPage.
        /// </summary>
        public RelayCommand GoToAlbumsPage
        {
            get
            {
                return goToAlbumsPage
                    ?? (goToAlbumsPage = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo(ViewNames.AlbumsView);
                    }));
            }
        }

        private RelayCommand goToArtistsPage;

        /// <summary>
        /// Gets the GoToArtistsPage.
        /// </summary>
        public RelayCommand GoToArtistsPage
        {
            get
            {
                return goToArtistsPage
                    ?? (goToArtistsPage = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo(ViewNames.ArtistsView);
                    }));
            }
        }

        private RelayCommand goToFoldersPage;

        /// <summary>
        /// Gets the GoToFoldersPage.
        /// </summary>
        public RelayCommand GoToFoldersPage
        {
            get
            {
                return goToFoldersPage
                    ?? (goToFoldersPage = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo(ViewNames.FoldersView);
                    }));
            }
        }

        private RelayCommand goToGenresPage;

        /// <summary>
        /// Gets the GoToGenresPage.
        /// </summary>
        public RelayCommand GoToGenresPage
        {
            get
            {
                return goToGenresPage
                    ?? (goToGenresPage = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo(ViewNames.GenresView);
                    }));
            }
        }

        private RelayCommand goToPlaylistsPage;

        /// <summary>
        /// Gets the GoToPlaylistsPage.
        /// </summary>
        public RelayCommand GoToPlaylistsPage
        {
            get
            {
                return goToPlaylistsPage
                    ?? (goToPlaylistsPage = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo(ViewNames.PlaylistsView);
                    }));
            }
        }

        private RelayCommand goToSettingsPage;

        /// <summary>
        /// Gets the GoToSettingsPage.
        /// </summary>
        public RelayCommand GoToSettingsPage
        {
            get
            {
                return goToSettingsPage
                    ?? (goToSettingsPage = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo(ViewNames.SettingsView);
                    }));
            }
        }

        private RelayCommand goToSearchPage;

        /// <summary>
        /// Gets the GoToSearchPage.
        /// </summary>
        public RelayCommand GoToSearchPage
        {
            get
            {
                return goToSearchPage
                    ?? (goToSearchPage = new RelayCommand(
                    () =>
                    {
                         navigationService.NavigateTo(ViewNames.SearchView);
                    }));
            }
        }

        
        #endregion
        #region Commands

        private RelayCommand goToNowPlaying;

        /// <summary>
        /// Gets the GoToNowPlaying.
        /// </summary>
        public RelayCommand GoToNowPlaying
        {
            get
            {
                return goToNowPlaying
                    ?? (goToNowPlaying = new RelayCommand(
                    () =>
                    {
                        if (CurrentSongIndex > -1)
                        {
                            if (MediaPlayerState.Closed == BackgroundMediaPlayer.Current.CurrentState)
                            {
                                //NextPlayerDataLayer.Diagnostics.Logger.Save("FG Main closed");
                                //NextPlayerDataLayer.Diagnostics.Logger.SaveToFile();
                                navigationService.NavigateTo(ViewNames.NowPlayingView, "start");
                            }
                            else
                            {
                                if (!NextPlayer.Common.SuspensionManager.SessionState.ContainsKey("mainpage"))
                                {
                                    NextPlayer.Common.SuspensionManager.SessionState.Add("mainpage", true);
                                }
                                //NextPlayerDataLayer.Diagnostics.Logger.Save("FG Main not closed");
                                //NextPlayerDataLayer.Diagnostics.Logger.SaveToFile();
                                navigationService.NavigateTo(ViewNames.NowPlayingView);
                            }

                        }
                    }));
            }
        }

        private RelayCommand playClick;

        /// <summary>
        /// Gets the PlayClick.
        /// </summary>
        public RelayCommand PlayClick
        {
            get
            {
                return playClick
                    ?? (playClick = new RelayCommand(
                    () =>
                    {
                        if (CurrentSongIndex > -1)
                        {
                            Play();
                        }
                    }));
            }
        }

        private RelayCommand nextClick;

        /// <summary>
        /// Gets the NextClick.
        /// </summary>
        public RelayCommand NextClick
        {
            get
            {
                return nextClick
                    ?? (nextClick = new RelayCommand(
                    () =>
                    {
                        if (CurrentSongIndex > -1)
                        {
                            Next();
                        }
                    }));
            }
        }

        private RelayCommand stop;

        /// <summary>
        /// Gets the Stop.
        /// </summary>
        public RelayCommand Stop
        {
            get
            {
                return stop
                    ?? (stop = new RelayCommand(
                    () =>
                    {
                        if (IsMyBackgroundTaskRunning)
                        {
                            //SendMessage(AppConstants.ShutdownBGPlayer);
                            //RemoveMediaPlayerEventHandlers();
                            SendMessage(AppConstants.ShutdownBGPlayer);
                        }
                    }));
            }
        }

        #endregion




        public void Activate(object parameter, Dictionary<string, object> state)
        {
            //NextPlayerDataLayer.Diagnostics.Logger.Save("FG Main activate");
            //NextPlayerDataLayer.Diagnostics.Logger.SaveToFile();
            int index = CurrentSongIndex;
            if (index > -1 && Library.Current.NowPlayingList.Count > 0)
            {
                SongItem song = Library.Current.NowPlayingList.ElementAt(index);
                Title = song.Title;
                Artist = song.Artist;
                SetCover(song.Path);
            }
            else
            {
                Title = "-";
                Artist = "-";
                SetDefaultCover();
            }
            //StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
            if (IsMyBackgroundTaskRunning)
            {
                AddMediaPlayerEventHandlers();
            }
            else
            {
                //StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
            }

            if (ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.TilePlay) != null)
            {
                if (IsMyBackgroundTaskRunning)
                {
                    SendMessage(AppConstants.NowPlayingListChanged);
                    SendMessage(AppConstants.StartPlayback, 0);
                }
                else
                {
                    StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
                }
            }
        }

        public void Deactivate(Dictionary<string, object> state)
        {
            RemoveMediaPlayerEventHandlers();
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }

        public void ForegroundApp_Resuming(object sender, object e)
        {
            if (IsMyBackgroundTaskRunning)
            {
                AddMediaPlayerEventHandlers();
            }
        }

        public void ForegroundApp_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            if (IsMyBackgroundTaskRunning)
            {
                RemoveMediaPlayerEventHandlers();
            }
        }

        private void RemoveMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.MessageReceivedFromBackground -= this.BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        private void AddMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.MessageReceivedFromBackground += this.BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            foreach (string key in e.Data.Keys)
            {
                switch (key)
                {
                    case AppConstants.SongIndex:
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            CurrentSongIndex = Int32.Parse(e.Data[key].ToString());
                            try
                            {
                                SongItem song = Library.Current.NowPlayingList.ElementAt(CurrentSongIndex);
                                SetCover(song.Path);
                                Artist = song.Artist;
                                Title = song.Title;
                            }
                            catch (System.IndexOutOfRangeException)
                            {
                                SetDefaultCover();
                                Artist = "-";
                                Title = "-";
                            }
                        });
                        break;
                    //case AppConstants.PlayerClosed://! mozna usunac
                    //    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    //    {
                    //        RemoveMediaPlayerEventHandlers();
                    //    });
                    //    break;
                    case AppConstants.BackgroundTaskStarted:
                        SererInitialized.Set();
                        break;
                }
            }
        }


        public void Play()
        {
            if (IsMyBackgroundTaskRunning)
            {
                if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
                {
                    SendMessage(AppConstants.Pause);
                }
                else if (MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState)
                {
                    SendMessage(AppConstants.Play);
                }
                else if (MediaPlayerState.Closed == BackgroundMediaPlayer.Current.CurrentState)
                {
                    if (CurrentSongIndex > -1 && CurrentSongIndex < Library.Current.NowPlayingList.Count)
                    {
                        SendMessage(AppConstants.NowPlayingListChanged);
                        SendMessage(AppConstants.StartPlayback, CurrentSongIndex);
                    }
                }
            }
            else
            {
                if (CurrentSongIndex > -1 && CurrentSongIndex < Library.Current.NowPlayingList.Count)
                {
                    StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
                }
            }
        }

        public void Next()
        {
            if (IsMyBackgroundTaskRunning)
            {
                if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
                {
                    SendMessage(AppConstants.SkipNext);
                }
                else if (MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState)
                {
                    SendMessage(AppConstants.SkipNext);
                }
                else if (MediaPlayerState.Closed == BackgroundMediaPlayer.Current.CurrentState)
                {
                    if (CurrentSongIndex > -1 && CurrentSongIndex < Library.Current.NowPlayingList.Count)
                    {
                        RemoveMediaPlayerEventHandlers();
                        AddMediaPlayerEventHandlers();
                        SendMessage(AppConstants.SkipNext);
                    }
                }
            }
            else
            {
                if (CurrentSongIndex > -1 && CurrentSongIndex < Library.Current.NowPlayingList.Count)
                {
                    StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
                }
            }
        }

        private void SendMessage(string constants)// SendMessage(constants,"")
        {
            var value = new ValueSet();
            value.Add(constants, "");
            BackgroundMediaPlayer.SendMessageToBackground(value);
        }

        public void SendMessage(string constants, object value)
        {
            var message = new ValueSet();
            message.Add(constants, value);
            BackgroundMediaPlayer.SendMessageToBackground(message);
        }

        private async void SetCover(string path)
        {
            Cover = await Library.Current.GetCoverSmall(path);
        }

        private async void SetDefaultCover()
        {
            Cover = await Library.Current.GetDefaultSmallCover();
        }

        private void StartBackgroundAudioTask(string s, object o)
        {
            BackgroundMediaPlayer.MessageReceivedFromBackground += this.BackgroundMediaPlayer_MessageReceivedFromBackground;
            var backgroundtaskinitializationresult = Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                bool result = SererInitialized.WaitOne(10000);
                if (result == true)
                {
                    SendMessage(s, o);
                }
                else
                {
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.BackgroundTaskState, AppConstants.BackgroundTaskCancelled);
                    throw new Exception("Background Audio Task didn't start in expected time");
                }
            });
        }

    }
}
