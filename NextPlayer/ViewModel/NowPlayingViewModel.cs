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
using Windows.UI.Xaml;
using Windows.Media.Playback;
using Windows.Foundation.Collections;
using NextPlayerDataLayer.Constants;
using NextPlayerDataLayer.Helpers;
using Windows.UI.Xaml.Media.Imaging;
using NextPlayer.Converters;
using Windows.Foundation;
using System.Threading;
using Windows.UI.Core;
using GalaSoft.MvvmLight.Threading;
using Windows.UI.Xaml.Media;

namespace NextPlayer.ViewModel
{
    public class NowPlayingViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private int songId;
        private int index;
        private bool fromDB = false;
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

        public NowPlayingViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            SererInitialized = new AutoResetEvent(false);
            _timer = new DispatcherTimer();
            //Library.Current.Save("NPVM konstruktor");
            App.Current.Suspending += ForegroundApp_Suspending;
            App.Current.Resuming += ForegroundApp_Resuming;
            //SetupTimer();
        }

        #region Properties
        /// <summary>
        /// The <see cref="CurrentNr" /> property's name.
        /// </summary>
        public const string CurrentNrPropertyName = "CurrentNr";

        private int currentNr = 0;

        /// <summary>
        /// Sets and gets the CurrentNr property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int CurrentNr
        {
            get
            {
                if (IsInDesignMode) currentNr = 10;
                return currentNr;
            }

            set
            {
                if (currentNr == value)
                {
                    return;
                }

                currentNr = value;
                RaisePropertyChanged(CurrentNrPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SongsCount" /> property's name.
        /// </summary>
        public const string SongsCountPropertyName = "SongsCount";

        private int songsCount = 0;

        /// <summary>
        /// Sets and gets the SongsCount property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int SongsCount
        {
            get
            {
                if (IsInDesignMode) songsCount = 25;
                return songsCount;
            }

            set
            {
                if (songsCount == value)
                {
                    return;
                }

                songsCount = value;
                RaisePropertyChanged(SongsCountPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Title" /> property's name.
        /// </summary>
        public const string TitlePropertyName = "Title";

        private string title = "";

        /// <summary>
        /// Sets and gets the Title property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Title
        {
            get
            {
                if (IsInDesignMode) title = "Song title";
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

        private string artist = "";

        /// <summary>
        /// Sets and gets the Artist property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Artist
        {
            get
            {
                if (IsInDesignMode) artist = "Artist";
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
                if (IsInDesignMode) album = "Album";
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
        /// The <see cref="CurrentTime" /> property's name.
        /// </summary>
        public const string CurrentTimePropertyName = "CurrentTime";

        private TimeSpan currentTime = TimeSpan.Zero;

        /// <summary>
        /// Sets and gets the CurrentTime property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public TimeSpan CurrentTime
        {
            get
            {
                return currentTime;
            }

            set
            {
                if (currentTime == value)
                {
                    if (IsInDesignMode) currentTime = new TimeSpan(0, 5, 39);
                    return;
                }

                currentTime = value;
                RaisePropertyChanged(CurrentTimePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="EndTime" /> property's name.
        /// </summary>
        public const string EndTimePropertyName = "EndTime";

        private TimeSpan endTime = TimeSpan.Zero;

        /// <summary>
        /// Sets and gets the EndTime property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public TimeSpan EndTime
        {
            get
            {
                if (IsInDesignMode) currentTime = new TimeSpan(0, 5, 39);
                return endTime;
            }

            set
            {
                if (endTime == value)
                {
                    return;
                }

                endTime = value;
                RaisePropertyChanged(EndTimePropertyName);
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
        /// The <see cref="Rating" /> property's name.
        /// </summary>
        public const string RatingPropertyName = "Rating";

        private int rating = 0;

        /// <summary>
        /// Sets and gets the Rating property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Rating
        {
            get
            {
                return rating;
            }

            set
            {
                if (rating == value)
                {
                    return;
                }
                if (value == 0 || value == 1 || value == 2 || value == 3 || value == 4 || value == 5)
                {
                    rating = value;

                    UpdateRating();

                    fromDB = false;
                    RaisePropertyChanged(RatingPropertyName);
                }
            }
        }

        /// <summary>
        /// The <see cref="ProgressBarValue" /> property's name.
        /// </summary>
        public const string ProgressBarValuePropertyName = "ProgressBarValue";

        private double progressBarValue = 0.0;

        /// <summary>
        /// Sets and gets the ProgressBarValue property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double ProgressBarValue
        {
            get
            {
                if (IsInDesignMode) progressBarValue = 40.0;
                return progressBarValue;
            }

            set
            {
                if (progressBarValue == value)
                {
                    return;
                }

                progressBarValue = value;
                RaisePropertyChanged(ProgressBarValuePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ProgressBarMaxValue" /> property's name.
        /// </summary>
        public const string ProgressBarMaxValuePropertyName = "ProgressBarMaxValue";

        private double progressBarMaxValue = 0.0;

        /// <summary>
        /// Sets and gets the ProgressBarMaxValue property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double ProgressBarMaxValue
        {
            get
            {
                if (IsInDesignMode) progressBarValue = 80.0;
                return progressBarMaxValue;
            }

            set
            {
                if (progressBarMaxValue == value)
                {
                    return;
                }

                progressBarMaxValue = value;
                RaisePropertyChanged(ProgressBarMaxValuePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="PlayButtonContent" /> property's name.
        /// </summary>
        public const string PlayButtonContentPropertyName = "PlayButtonContent";

        private string playButtonContent = "\uE17e\uE102";

        /// <summary>
        /// Sets and gets the PlayButtonContent property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string PlayButtonContent
        {
            get
            {
                return playButtonContent;
            }

            set
            {
                if (playButtonContent == value)
                {
                    return;
                }

                playButtonContent = value;
                RaisePropertyChanged(PlayButtonContentPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="RepeatButtonContent" /> property's name.
        /// </summary>
        public const string RepeatButtonContentPropertyName = "RepeatButtonContent";

        private string repeatButtonContent = "\uE1cd";

        /// <summary>
        /// Sets and gets the RepeatButtonContent property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string RepeatButtonContent
        {
            get
            {
                if (IsInDesignMode) repeatButtonContent = "&#xE1cd;";
                return repeatButtonContent;
            }

            set
            {
                if (repeatButtonContent == value)
                {
                    return;
                }

                repeatButtonContent = value;
                RaisePropertyChanged(RepeatButtonContentPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="RepeatButtonForegorund" /> property's name.
        /// </summary>
        public const string RepeatButtonForegroundPropertyName = "RepeatButtonForeground";

        private SolidColorBrush repeatButtonForeground = new SolidColorBrush(Windows.UI.Colors.Gray);

        /// <summary>
        /// Sets and gets the RepeatButtonForegorund property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public SolidColorBrush RepeatButtonForeground
        {
            get
            {
                return repeatButtonForeground;
            }

            set
            {
                if (repeatButtonForeground == value)
                {
                    return;
                }

                repeatButtonForeground = value;
                RaisePropertyChanged(RepeatButtonForegroundPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ShuffleButtonForeground" /> property's name.
        /// </summary>
        public const string ShuffleButtonForegroundPropertyName = "ShuffleButtonForeground";

        private SolidColorBrush shuffleButtonForeground = new SolidColorBrush(Windows.UI.Colors.Gray);

        /// <summary>
        /// Sets and gets the ShuffleButtonForeground property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public SolidColorBrush ShuffleButtonForeground
        {
            get
            {
                return shuffleButtonForeground;
            }

            set
            {
                if (shuffleButtonForeground == value)
                {
                    return;
                }

                shuffleButtonForeground = value;
                RaisePropertyChanged(ShuffleButtonForegroundPropertyName);
            }
        }
        #endregion

        #region Buttons Commands
        
        private RelayCommand previousButtonClick;

        /// <summary>
        /// Gets the PreviousButtonClick.
        /// </summary>
        public RelayCommand PreviousButtonClick
        {
            get
            {
                return previousButtonClick
                    ?? (previousButtonClick = new RelayCommand(
                    () =>
                    {
                        Previous();
                    }));
            }
        }

        private RelayCommand playButtonClick;

        /// <summary>
        /// Gets the PlayButtonClick.
        /// </summary>
        public RelayCommand PlayButtonClick
        {
            get
            {
                return playButtonClick
                    ?? (playButtonClick = new RelayCommand(
                    () =>
                    {
                        Play();
                    }));
            }
        }

        private RelayCommand nextButtonClick;

        /// <summary>
        /// Gets the NextButtonClick.
        /// </summary>
        public RelayCommand NextButtonClick
        {
            get
            {
                return nextButtonClick
                    ?? (nextButtonClick = new RelayCommand(
                    () =>
                    {
                        Next();
                    }));
            }
        }

        private RelayCommand shuffle_Click;

        /// <summary>
        /// Gets the Shuffle_Click.
        /// </summary>
        public RelayCommand Shuffle_Click
        {
            get
            {
                return shuffle_Click
                    ?? (shuffle_Click = new RelayCommand(
                    () =>
                    {
                        Shuffle.Change();
                        ShuffleButtonForeground = Shuffle.CurrentStateColor();
                        SendMessage(AppConstants.Shuffle);
                    }));
            }
        }

        private RelayCommand repeat_Click;

        /// <summary>
        /// Gets the Repeat_Click.
        /// </summary>
        public RelayCommand Repeat_Click
        {
            get
            {
                return repeat_Click
                    ?? (repeat_Click = new RelayCommand(
                    () =>
                    {
                        Repeat.Change();
                        RepeatButtonContent = Repeat.CurrentStateContent(); //repeat.ToString();
                        RepeatButtonForeground = Repeat.CurrentStateColor();
                        SendMessage(AppConstants.Repeat);
                    }));
            }
        }
        private RelayCommand showLyricsclick;

        /// <summary>
        /// Gets the ShowLyricsClick.
        /// </summary>
        public RelayCommand ShowLyricsClick
        {
            get
            {
                return showLyricsclick
                    ?? (showLyricsclick = new RelayCommand(
                    () =>
                    {
                        if (!NextPlayer.Common.SuspensionManager.SessionState.ContainsKey("lyrics"))
                        {
                            NextPlayer.Common.SuspensionManager.SessionState.Add("lyrics", true);
                        }
                        String[] s = new String[3];
                        s[0] = Artist;
                        s[1] = Title;
                        s[2] = songId.ToString();
                        navigationService.NavigateTo(ViewNames.LyricsView, ParamConvert.ToString(s));
                    }));
            }
        }

        private RelayCommand goToNowPlayingList;

        /// <summary>
        /// Gets the GoToNowPlayingList.
        /// </summary>
        public RelayCommand GoToNowPlayingList
        {
            get
            {
                return goToNowPlayingList
                    ?? (goToNowPlayingList = new RelayCommand(
                    () =>
                    {
                        if (!NextPlayer.Common.SuspensionManager.SessionState.ContainsKey("nplist"))
                        {
                            NextPlayer.Common.SuspensionManager.SessionState.Add("nplist", true);
                        }
                        navigationService.NavigateTo(ViewNames.PlaylistView);
                    }));
            }
        }

        #endregion

        public void Activate(object parameter, Dictionary<string, object> state)
        {
            
            //App.Current.Suspending += ForegroundApp_Suspending;
            //App.Current.Resuming += ForegroundApp_Resuming;
            
            
            index = CurrentSongIndex;
            if (index > -1)
            {
                SongItem song = Library.Current.NowPlayingList.ElementAt(index);
                CurrentNr = index + 1;
                SongsCount = Library.Current.NowPlayingList.Count;
                songId = song.SongId;
                Title = song.Title;
                Artist = song.Artist;
                Album = song.Album;
                fromDB = true;
                Rating = song.Rating;
                SetCover(song.Path);

                SetupTimer();

                RepeatButtonContent = Repeat.CurrentStateContent();
                RepeatButtonForeground = Repeat.CurrentStateColor();
                ShuffleButtonForeground = Shuffle.CurrentStateColor();

                if (IsMyBackgroundTaskRunning)
                {
                    //Library.Current.Save("BG running");

                    AddMediaPlayerEventHandlers();

                    if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
                    {
                        PlayButtonContent = "\uE17e\uE103";//pause
                    }

                    object r = ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.ResumePlayback);
                    if (r != null)
                    {
                        SendMessage(AppConstants.ResumePlayback);
                        TimeSpan t = BackgroundMediaPlayer.Current.NaturalDuration;
                        double absvalue = (int)Math.Round(t.TotalSeconds - 0.5, MidpointRounding.AwayFromZero);
                        ProgressBarMaxValue = absvalue;
                        EndTime = BackgroundMediaPlayer.Current.NaturalDuration;
                    }
                    else if (NextPlayer.Common.SuspensionManager.SessionState.ContainsKey("lyrics"))//mozna chyba zmienic na Dict<> state
                    {
                        NextPlayer.Common.SuspensionManager.SessionState.Remove("lyrics");
                    }
                    else if (NextPlayer.Common.SuspensionManager.SessionState.ContainsKey("nplist"))//mozna chyba zmienic na Dict<> state
                    {
                        NextPlayer.Common.SuspensionManager.SessionState.Remove("nplist");
                    }
                    else if (NextPlayer.Common.SuspensionManager.SessionState.ContainsKey("mainpage"))//mozna chyba zmienic na Dict<> state
                    {
                        NextPlayer.Common.SuspensionManager.SessionState.Remove("mainpage");

                        TimeSpan t = BackgroundMediaPlayer.Current.NaturalDuration;
                        double absvalue = (int)Math.Round(t.TotalSeconds - 0.5, MidpointRounding.AwayFromZero);
                        ProgressBarMaxValue = absvalue;
                        EndTime = BackgroundMediaPlayer.Current.NaturalDuration;
                    }
                    else
                    {
                        SendMessage(AppConstants.NowPlayingListChanged);
                        SendMessage(AppConstants.StartPlayback, CurrentSongIndex);
                    }
                }
                else
                {
                    if (NextPlayer.Common.SuspensionManager.SessionState.ContainsKey("mainpage"))
                    {
                        NextPlayer.Common.SuspensionManager.SessionState.Remove("mainpage");
                    }
                    object r =  ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.ResumePlayback);
                    //if (r != null)
                    //{
                    //    StartBackgroundAudioTask(AppConstants.ResumePlayback, CurrentSongIndex);
                    //}
                    //else
                    //{
                        StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
                    //}
                }
                StartTimer();
            }
            else
            {
                navigationService.NavigateTo(ViewNames.MainView);
            }
            //if (parameter != null)
            //{
                
            //    if (parameter.GetType() == typeof(int))
            //    {
            //        songId = (int)parameter;
            //    }
            //}
        }

        public void Deactivate(Dictionary<string, object> state)
        {
            if (IsMyBackgroundTaskRunning)
            {
                RemoveMediaPlayerEventHandlers();
            }
            StopTimer();
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }
        #region Foreground App Lifecycle Handlers
        /// <summary>
        /// Sends message to background informing app has resumed
        /// Subscribe to MediaPlayer events
        /// </summary>
        public void ForegroundApp_Resuming(object sender, object e)
        {
            //Library.Current.Save("foreground resumed");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppState, AppConstants.ForegroundAppActive);

            SongItem song = Library.Current.NowPlayingList.ElementAt(CurrentSongIndex);
            CurrentNr = CurrentSongIndex + 1;
            songId = song.SongId;
            SetCover(song.Path);
            Artist = song.Artist;
            Album = song.Album;
            Title = song.Title;
            fromDB = true;
            Rating = song.Rating;

            if (IsMyBackgroundTaskRunning)
            {
                AddMediaPlayerEventHandlers();

                StartTimer();
                
                TimeSpan t = BackgroundMediaPlayer.Current.NaturalDuration;
                double absvalue = (int)Math.Round(t.TotalSeconds - 0.5, MidpointRounding.AwayFromZero);
                ProgressBarMaxValue = absvalue;
                EndTime = BackgroundMediaPlayer.Current.NaturalDuration;

                SendMessage(AppConstants.AppResumed, DateTime.Now.ToString());

                if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
                {
                    PlayButtonContent = "\uE17e\uE103";//pause
                }
                else
                {
                    PlayButtonContent = "\uE17e\uE102";//play
                }
            }
            else
            {
                PlayButtonContent = "\uE17e\uE102";//play
            }
        }
        /// <summary>
        /// Send message to Background process that app is to be suspended
        /// Stop clock and slider when suspending
        /// Unsubscribe handlers for MediaPlayer events
        /// </summary>
        public void ForegroundApp_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            StopTimer();
            if (IsMyBackgroundTaskRunning)
            {
                RemoveMediaPlayerEventHandlers();
            }
            var deferral = e.SuspendingOperation.GetDeferral();
            //Library.Current.Save("suspending");
            
            SendMessage(AppConstants.AppSuspended, DateTime.Now.ToString());
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppState, AppConstants.ForegroundAppSuspended);

            deferral.Complete();
        }
        #endregion

        #region Media Playback Helper methods
        /// <summary>
        /// Unsubscribes to MediaPlayer events. Should run only on suspend
        /// </summary>
        private void RemoveMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.Current.CurrentStateChanged -= this.MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground -= this.BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        /// <summary>
        /// Subscribes to MediaPlayer events
        /// </summary>
        private void AddMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.Current.CurrentStateChanged += this.MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground += this.BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        /// <summary>
        /// Initialize Background Media Player Handlers and starts playback
        /// </summary>
        private void StartBackgroundAudioTask(string s, object o)
        {
            //IAsyncAction backgroundtaskinitializationresult = ;
            BackgroundMediaPlayer.MessageReceivedFromBackground += this.BackgroundMediaPlayer_MessageReceivedFromBackground;
            var backgroundtaskinitializationresult = Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync( CoreDispatcherPriority.Normal,() =>
                {
                    bool result = SererInitialized.WaitOne(10000);
                    if (result == true)
                    {
                        SendMessage(s, o);
                        BackgroundMediaPlayer.MessageReceivedFromBackground -= this.BackgroundMediaPlayer_MessageReceivedFromBackground;
                        AddMediaPlayerEventHandlers();
                    }
                    else
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(AppConstants.BackgroundTaskState, AppConstants.BackgroundTaskCancelled);
                        throw new Exception("Background Audio Task didn't start in expected time");
                    }
                });
            
            //Task.Run(() =>
            //{
            //    bool result = SererInitialized.WaitOne(10000);
            //    if (result == true)
            //    {
            //        SendMessage(s, o);
            //        BackgroundMediaPlayer.MessageReceivedFromBackground -= this.BackgroundMediaPlayer_MessageReceivedFromBackground;
            //        AddMediaPlayerEventHandlers();
            //    }
            //    else
            //    {
            //        ApplicationSettingsHelper.SaveSettingsValue(AppConstants.BackgroundTaskState, AppConstants.BackgroundTaskCancelled);
            //        throw new Exception("Background Audio Task didn't start in expected time");
            //    }
            //}
            //);
            //StartTimer();
            backgroundtaskinitializationresult.Completed = BackgroundTaskInitializationCompleted;
        }

        private void BackgroundTaskInitializationCompleted(IAsyncAction action, AsyncStatus status)
        {
            if (status == AsyncStatus.Completed)
            {
                NextPlayerDataLayer.Diagnostics.Logger.Save("Background Audio Task initialized");
            }
            else if (status == AsyncStatus.Error)
            {
                NextPlayerDataLayer.Diagnostics.Logger.Save("Background Audio Task could not initialized due to an error ::" + action.ErrorCode.ToString());
            }
            NextPlayerDataLayer.Diagnostics.Logger.SaveToFile();
        }
        #endregion

        #region Background MediaPlayer Event handlers
        /// <summary>
        /// MediaPlayer state changed event handlers. 
        /// Note that we can subscribe to events even if Media Player is playing media in background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            switch (sender.CurrentState)
            {
                case MediaPlayerState.Playing:
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        PlayButtonContent = "\uE17e\uE103";// Change to pause button

                    });
                    break;
                case MediaPlayerState.Paused:
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        PlayButtonContent = "\uE17e\uE102";     // Change to play button

                    });
                    break;
                //case MediaPlayerState.Stopped:
                //    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                //    {
                //        StopTimer();
                //        ProgressBarValue = 0.0;
                //        CurrentTime = TimeSpan.Zero;
                //        EndTime = TimeSpan.Zero;
                //    });
                //    break;
                //case MediaPlayerState.Closed:
                //    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                //    {
                //        StopTimer();
                //        ProgressBarValue = 0.0;
                //        CurrentTime = TimeSpan.Zero;
                //        //EndTime = TimeSpan.Zero;
                //        PlayButtonContent = "\uE17e\uE102"; 
                //    });
                //    break;
            }
        }

        /// <summary>
        /// This event fired when a message is recieved from Background Process
        /// </summary>
        void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            foreach (string key in e.Data.Keys)
            {
                switch (key)
                {
                    case AppConstants.SongIndex:
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            CurrentNr = Int32.Parse(e.Data[key].ToString()) + 1;
                            CurrentSongIndex = Int32.Parse(e.Data[key].ToString());
                            try
                            {
                                SongItem song = Library.Current.NowPlayingList.ElementAt(CurrentSongIndex);
                                songId = song.SongId;
                                SetCover(song.Path);
                                Artist = song.Artist;
                                Album = song.Album;
                                Title = song.Title;
                                fromDB = true;
                                Rating = song.Rating;
                            }
                            catch(System.IndexOutOfRangeException ex)
                            {
                                //SongItem song = Library.Current.NowPlayingList.ElementAt(CurrentSongIndex);
                                songId = -1;
                                //SetCover(song.Path);
                                Artist = "-";
                                Album = "-";
                                Title = "-";
                                fromDB = true;
                                Rating = 0;
                                NextPlayerDataLayer.Diagnostics.Logger.Save("NP View MesRecFromBG SongIndex IndexOutOfRangeException" + "\n"
                                    + ex.Message + "\n"
                                    + "NPList Count " + Library.Current.NowPlayingList.Count + " index " + CurrentSongIndex);
                                NextPlayerDataLayer.Diagnostics.Logger.SaveToFile();
                            }
                            catch (Exception ex)
                            {
                                songId = -1;
                                Artist = "-";
                                Album = "-";
                                Title = "-";
                                fromDB = true;
                                Rating = 0;
                                NextPlayerDataLayer.Diagnostics.Logger.Save("NP View MesRecFromBG SongIndex" + "\n"
                                    + ex.Message + "\n"
                                    + "NPList Count " + Library.Current.NowPlayingList.Count + " index " + CurrentSongIndex);
                                NextPlayerDataLayer.Diagnostics.Logger.SaveToFile();
                            }
                        });
                        break;
                    case AppConstants.MediaOpened:
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            TimeSpan t = BackgroundMediaPlayer.Current.NaturalDuration;
                            double absvalue = (int)Math.Round(t.TotalSeconds - 0.5, MidpointRounding.AwayFromZero);
                            ProgressBarMaxValue = absvalue;                           
                            ProgressBarValue = 0.0;
                            CurrentTime = TimeSpan.Zero;
                            EndTime = BackgroundMediaPlayer.Current.NaturalDuration;
                        });
                        break;
                    case AppConstants.Position:
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            TimeSpan result;
                            TimeSpan.TryParse(e.Data[key].ToString(), out result);
                            ProgressBarValue = result.Seconds;
                            CurrentTime = result;
                            EndTime = BackgroundMediaPlayer.Current.NaturalDuration;
                        });
                        break;
                    case AppConstants.PlayerClosed:
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            StopTimer();
                            RemoveMediaPlayerEventHandlers();
                            //ProgressBarValue = 0.0;
                            //CurrentTime = TimeSpan.Zero;
                            //EndTime = TimeSpan.Zero;
                            //PlayButtonContent = "\uE17e\uE102";
                        });
                        break;
                    case AppConstants.BackgroundTaskStarted:
                        //Wait for Background Task to be initialized before starting playback
                        SererInitialized.Set();
                        break;
                }
            }
        }

        #endregion

        public void Previous()
        {
            if (IsMyBackgroundTaskRunning)
            {
                SendMessage(AppConstants.SkipPrevious);
            }
            else
            {
                StartBackgroundAudioTask(AppConstants.SkipPrevious, "");
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
                    StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
                }
            }
            else
            {
                StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
            }
        }
        public void Next()
        {
            if (IsMyBackgroundTaskRunning)
            {
                SendMessage(AppConstants.SkipNext);
            }
            else
            {
                StartBackgroundAudioTask(AppConstants.SkipNext, "");
            }
        }
        
        #region Slider Timer

        public bool sliderpressed = false;
        private DispatcherTimer _timer;

        private void SetupTimer()
        {
            _timer.Interval = TimeSpan.FromSeconds(0.5);
            //_timer.Tick += _timer_Tick;
        }

        private void _timer_Tick(object sender, object e)
        {
            if (!sliderpressed)
            {
                ProgressBarValue = BackgroundMediaPlayer.Current.Position.TotalSeconds;
                CurrentTime = BackgroundMediaPlayer.Current.Position;
            }
            else
            {
                CurrentTime = TimeSpan.FromSeconds(ProgressBarValue);
            }
        }

        private void StartTimer()
        {
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer.Stop();
            _timer.Tick -= _timer_Tick;
        }


        private void videoMediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // get HRESULT from event args 
            string hr = GetHresultFromErrorMessage(e);
            // Handle media failed event appropriately 
        }

        private string GetHresultFromErrorMessage(ExceptionRoutedEventArgs e)
        {
            String hr = String.Empty;
            String token = "HRESULT - ";
            const int hrLength = 10;     // eg "0xFFFFFFFF"

            int tokenPos = e.ErrorMessage.IndexOf(token, StringComparison.Ordinal);
            if (tokenPos != -1)
            {
                hr = e.ErrorMessage.Substring(tokenPos + token.Length, hrLength);
            }

            return hr;
        }

        #endregion


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
            Cover = await Library.Current.GetCover(path, false);
        }

        private async void UpdateRating()
        {
            if (!fromDB)
            {
                Library.Current.ChangeRating(rating, CurrentSongIndex);
                if (App.LastFmRateOn)
                {
                    if (rating >= App.LastFmLove)
                    {
                        await LastFmManager.Current.TrackLove(Artist, Title);
                    }
                    if (rating<= App.LastFmUnLove)
                    {
                        await LastFmManager.Current.TrackUnlove(Artist, Title);
                    }
                }
                //Library.Current.NowPlayingList.ElementAt(CurrentSongIndex).Rating = rating;
                await DatabaseManager.UpdateSongRating(songId, rating);
                await MediaImport.UpdateRating(songId, rating);
            }
        }
    }
}