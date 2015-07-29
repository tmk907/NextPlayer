using NextPlayer.Common;
using NextPlayer.Converters;
using NextPlayerDataLayer.Constants;
using NextPlayerDataLayer.Helpers;
using NextPlayerDataLayer.Model;
using NextPlayerDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace NextPlayer.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NowPlayingView : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private AutoResetEvent SererInitialized;
        private ObservableCollection<SongItem> npList;

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
                object value = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.SongIndex);
                if (value != null)
                {
                    return Int32.Parse(value.ToString());
                }
                else
                    return -1;
            }
            set
            {
                ApplicationSettingsHelper.SaveSongIndex((int)value);
            }
        }

        SongItem songItem;

        public NowPlayingView()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            SererInitialized = new AutoResetEvent(false);
            this.NavigationCacheMode = NavigationCacheMode.Required;

            npList = new ObservableCollection<SongItem>();
            _timer = new DispatcherTimer();
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {

            if (CurrentSongIndex != -1)
            {

                npList = Library.Current.NowPlayingList;
                SetupTimer();
                StartTimer();
                RepeatButton.Content = Repeat.CurrentStateContent();
                RepeatButton.Foreground = Repeat.CurrentStateColor();
                ShuffleButton.Foreground = Shuffle.CurrentStateColor();
                int index = ApplicationSettingsHelper.ReadSongIndex();
                SongItem song = npList.ElementAt(index);
                this.defaultViewModel["Song"] = song;
                NrTextBlock.Text = (index + 1).ToString();
                AllTextBlock.Text = npList.Count().ToString();
                
                
                SetCover(song.Path);
    
                

                
                //if (e.NavigationParameter != null) //powrot z lyricsview
                //{
                //    if (IsMyBackgroundTaskRunning) SendMessage(AppConstants.AppResumed, DateTime.Now.ToString());
                //}
                //else 
                if (IsMyBackgroundTaskRunning)
                {
                    if (SuspensionManager.SessionState.ContainsKey("lyrics"))
                    {
                        SuspensionManager.SessionState.Remove("lyrics");
                    }
                    else
                    {
                        SendMessage(AppConstants.NowPlayingListChanged);
                        SendMessage(AppConstants.StartPlayback, CurrentSongIndex);
                    }
                }
                else
                {
                    StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
                }
            }
            else
            {
                Frame.Navigate(typeof(MainPage));
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            App.Current.Suspending += ForegroundApp_Suspending;
            App.Current.Resuming += ForegroundApp_Resuming;
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppState, AppConstants.ForegroundAppActive);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            StopTimer();
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        #region Buttons
        private void previousButton_Click(object sender, RoutedEventArgs e)
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

        private void playButton_Click(object sender, RoutedEventArgs e)
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

        private void nextButton_Click(object sender, RoutedEventArgs e)
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
        #endregion

        #region Foreground App Lifecycle Handlers
        /// <summary>
        /// Sends message to background informing app has resumed
        /// Subscribe to MediaPlayer events
        /// </summary>
        void ForegroundApp_Resuming(object sender, object e)
        {
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppState, AppConstants.ForegroundAppActive);

            // Verify if the task was running before
            if (IsMyBackgroundTaskRunning)
            {

                //if yes, reconnect to media play handlers
                AddMediaPlayerEventHandlers();

                //send message to background task that app is resumed, so it can start sending notifications
                SendMessage(AppConstants.AppResumed, DateTime.Now.ToString());


                if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
                {
                    PlayButton.Content = "\uE17e\uE103";//pause
                }
                else
                {
                    PlayButton.Content = "\uE17e\uE102";//play
                }
                this.defaultViewModel["Song"] = Library.Current.GetFromNowPlaying(CurrentSongIndex);
            }
            else
            {
                PlayButton.Content = "\uE17e\uE102";//play
            }

        }
        /// <summary>
        /// Send message to Background process that app is to be suspended
        /// Stop clock and slider when suspending
        /// Unsubscribe handlers for MediaPlayer events
        /// </summary>
        void ForegroundApp_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            StopTimer();
            var deferral = e.SuspendingOperation.GetDeferral();
            SendMessage(AppConstants.AppSuspended, DateTime.Now.ToString());
            RemoveMediaPlayerEventHandlers();
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppState, AppConstants.ForegroundAppSuspended);
            deferral.Complete();
        }
        #endregion
        #region Background MediaPlayer Event handlers
        /// <summary>
        /// MediaPlayer state changed event handlers. 
        /// Note that we can subscribe to events even if Media Player is playing media in background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            switch (sender.CurrentState)
            {
                case MediaPlayerState.Playing:

                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        //StartTimer();
                        PlayButton.Content = "\uE17e\uE103";// Change to pause button
                    }
                        );

                    break;
                case MediaPlayerState.Paused:
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        //StopTimer();
                        PlayButton.Content = "\uE17e\uE102";     // Change to play button
                    }
                    );

                    break;
                case MediaPlayerState.Stopped:
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        StopTimer();
                        progressbar.Value = 0.0;
                        this.defaultViewModel["CurrentTime"] = TimeSpan.Zero;
                        this.defaultViewModel["EndTime"] = TimeSpan.Zero;
                        CurrentTime.Text = String.Format(@"{0:hh\:mm\:ss}", TimeSpan.Zero);
                        EndTime.Text = String.Format(@"{0:hh\:mm\:ss}", TimeSpan.Zero);
                    }
                    );
                    break;
            }
        }

        /// <summary>
        /// This event fired when a message is recieved from Background Process
        /// </summary>
        async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            foreach (string key in e.Data.Keys)
            {
                switch (key)
                {
                    case AppConstants.SongIndex:

                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            NrTextBlock.Text = (Int32.Parse(e.Data[key].ToString()) + 1).ToString();
                            CurrentSongIndex = Int32.Parse(e.Data[key].ToString());
                            SongItem song = npList.ElementAt(CurrentSongIndex);
                            SetCover(song.Path);
                            this.defaultViewModel["Song"] = song;
                            //try
                            //{
                            //    EndTime.Text = String.Format(@"{0:hh\:mm\:ss}", BackgroundMediaPlayer.Current.NaturalDuration).Remove(8);
                            //}
                            //catch
                            //{
                            //    EndTime.Text = String.Format(@"{0:hh\:mm\:ss}", BackgroundMediaPlayer.Current.NaturalDuration);
                            //}
                        }
                        );
                        break;
                    case AppConstants.MediaOpened:
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            TimeSpan t = BackgroundMediaPlayer.Current.NaturalDuration;
                            double absvalue = (int)Math.Round(t.TotalSeconds - 0.5, MidpointRounding.AwayFromZero);
                            progressbar.Maximum = absvalue;
                            progressbar.StepFrequency = 1.0;
                            progressbar.Value = 0.0;
                            this.defaultViewModel["CurrentTime"] = TimeSpan.Zero;
                            this.defaultViewModel["EndTime"] = BackgroundMediaPlayer.Current.NaturalDuration;
                            CurrentTime.Text = String.Format(@"{0:hh\:mm\:ss}", TimeSpan.Zero);
                            EndTime.Text = String.Format(@"{0:hh\:mm\:ss}", BackgroundMediaPlayer.Current.NaturalDuration);
                        }
                        );
                        break;
                    case AppConstants.Position:
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            TimeSpan result;
                            TimeSpan.TryParse(e.Data[key].ToString(), out result);
                            progressbar.Value = result.Seconds;
                            this.defaultViewModel["CurrentTime"] = result;
                            this.defaultViewModel["EndTime"] = BackgroundMediaPlayer.Current.NaturalDuration;
                            CurrentTime.Text = String.Format(@"{0:hh\:mm\:ss}", result);
                            EndTime.Text = String.Format(@"{0:hh\:mm\:ss}", BackgroundMediaPlayer.Current.NaturalDuration);
                        }
                        );
                        break;
                    case AppConstants.BackgroundTaskStarted:
                        //Wait for Background Task to be initialized before starting playback
                        SererInitialized.Set();
                        break;
                }
            }
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
            AddMediaPlayerEventHandlers();
            var backgroundtaskinitializationresult = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                bool result = SererInitialized.WaitOne(10000);
                //Send message to initiate playback
                if (result == true)
                {
                    SendMessage(s, o);
                }
                else
                {
                    throw new Exception("Background Audio Task didn't start in expected time");
                }
            }
            );
            backgroundtaskinitializationresult.Completed = new AsyncActionCompletedHandler(BackgroundTaskInitializationCompleted);
        }

        private void BackgroundTaskInitializationCompleted(IAsyncAction action, AsyncStatus status)
        {
            if (status == AsyncStatus.Completed)
            {
                //Debug.WriteLine("Background Audio Task initialized");
            }
            else if (status == AsyncStatus.Error)
            {
                //Debug.WriteLine("Background Audio Task could not initialized due to an error ::" + action.ErrorCode.ToString());
            }
        }
        #endregion


        private void SendMessage(string constants)
        {
            var value = new ValueSet();
            value.Add(constants, "");
            BackgroundMediaPlayer.SendMessageToBackground(value);
        }

        private void SendMessage(string constants, object value)
        {
            var message = new ValueSet();
            message.Add(constants, value);
            BackgroundMediaPlayer.SendMessageToBackground(message);
        }

        #region Slider Timer

        private DispatcherTimer _timer;
        private bool _sliderpressed = false;

        private void SetupTimer()
        {
            _timer.Interval = TimeSpan.FromSeconds(0.5);
        }

        private void _timer_Tick(object sender, object e)
        {
            if (!_sliderpressed)
            {
                //if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
                //{
                //    progressbar.Value = BackgroundMediaPlayer.Current.Position.TotalSeconds;
                //}
                progressbar.Value = BackgroundMediaPlayer.Current.Position.TotalSeconds;
                this.defaultViewModel["CurrentTime"] = BackgroundMediaPlayer.Current.Position;
                CurrentTime.Text = String.Format(@"{0:hh\:mm\:ss}", BackgroundMediaPlayer.Current.Position);
            }
            else
            {
                this.defaultViewModel["CurrentTime"] = TimeSpan.FromSeconds(progressbar.Value);
                CurrentTime.Text = String.Format(@"{0:hh\:mm\:ss}", TimeSpan.FromSeconds(progressbar.Value));
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

        void slider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _sliderpressed = true;
        }

        void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (TimeSpan.FromSeconds(progressbar.Value) > BackgroundMediaPlayer.Current.NaturalDuration)
            {
                SendMessage(AppConstants.Position, BackgroundMediaPlayer.Current.NaturalDuration.TotalSeconds);
            }
            SendMessage(AppConstants.Position, TimeSpan.FromSeconds(progressbar.Value));
            _sliderpressed = false;
        }

        void progressbar_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (!_sliderpressed)
            {
                SendMessage(AppConstants.Position, TimeSpan.FromSeconds(e.NewValue));
            }
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

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //progressbar.ValueChanged += progressbar_ValueChanged;

            PointerEventHandler pointerpressedhandler = new PointerEventHandler(slider_PointerEntered);
            progressbar.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);

            PointerEventHandler pointerreleasedhandler = new PointerEventHandler(slider_PointerCaptureLost);
            progressbar.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);
        }

        #endregion

        private async void SetCover(string path)
        {
            CoverImage.Source = await Library.Current.GetCover(path);
        }

        private void ShowLyrics_Click(object sender, RoutedEventArgs e)
        {
            if (!SuspensionManager.SessionState.ContainsKey("lyrics")) SuspensionManager.SessionState.Add("lyrics", true);
            String[] s = new String[2];
            s[0] = ArtistTextBox.Text;
            s[1] = TitleTextBox.Text;
            Frame.Navigate(typeof(LyricsView),  ParamConvert.ToString(s));
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            ShuffleButton.Foreground = Shuffle.CurrentStateColor();
            SendMessage(AppConstants.Shuffle);
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            Repeat.Change();
            RepeatButton.Content = Repeat.CurrentStateContent(); //repeat.ToString();
            RepeatButton.Foreground = Repeat.CurrentStateColor();
            SendMessage(AppConstants.Repeat);
        }


        private double x, y;
        
        private void Image_Pressed(object sender, PointerRoutedEventArgs e)
        {
            var  a = e.GetCurrentPoint(null);
            x = a.Position.X;
            y = a.Position.Y;
        }

        private void Image_Released(object sender, PointerRoutedEventArgs e)
        {
           
        }

        private void Image_Exited(object sender, PointerRoutedEventArgs e)
        {
            var a = e.GetCurrentPoint(null);
            if (Math.Abs(x - a.Position.X) > 50)
            {
                if (x - a.Position.X > 0) nextButton_Click(null, null);
                else previousButton_Click(null, null);
            }
        }

        private void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            playButton_Click(null, null);
        }
    }


}
