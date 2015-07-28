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

namespace NextPlayer.ViewModel
{
    public class NowPlayingViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private int songId;
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

        public NowPlayingViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

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







        public void Activate(object parameter, Dictionary<string, object> state)
        {
            if (parameter != null)
            {
                if (parameter.GetType() == typeof(int))
                {
                    songId = (int)parameter;
                }
            }
        }

        public void Deactivate(Dictionary<string, object> state)
        {
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }

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
                        if (IsMyBackgroundTaskRunning)
                        {
                            SendMessage(AppConstants.SkipPrevious);
                        }
                        else
                        {
                            //StartBackgroundAudioTask(AppConstants.SkipPrevious, "");
                        }
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
                                //StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
                            }
                        }
                        else
                        {
                            //StartBackgroundAudioTask(AppConstants.StartPlayback, CurrentSongIndex);
                        }
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
                        if (IsMyBackgroundTaskRunning)
                        {
                            SendMessage(AppConstants.SkipNext);
                        }
                        else
                        {
                            //StartBackgroundAudioTask(AppConstants.SkipNext, "");
                        }
                    }));
            }
        }





        public bool sliderpressed = false;

        //#region Slider Timer

        //private DispatcherTimer _timer;

        //private void SetupTimer()
        //{
        //    _timer.Interval = TimeSpan.FromSeconds(0.5);
        //}

        //private void _timer_Tick(object sender, object e)
        //{
        //    if (!sliderpressed)
        //    {
                
        //        progressbar.Value = BackgroundMediaPlayer.Current.Position.TotalSeconds;
        //        CurrentTime = BackgroundMediaPlayer.Current.Position;
        //    }
        //    else
        //    {
        //        CurrentTime = TimeSpan.FromSeconds(progressbar.Value);
        //    }
        //}

        //private void StartTimer()
        //{
        //    _timer.Tick += _timer_Tick;
        //    _timer.Start();
        //}

        //private void StopTimer()
        //{
        //    _timer.Stop();
        //    _timer.Tick -= _timer_Tick;
        //}

        //void slider_PointerEntered(object sender, PointerRoutedEventArgs e)
        //{
        //    sliderpressed = true;
        //}

        //void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        //{

        //    SendMessage(AppConstants.Position, TimeSpan.FromSeconds(progressbar.Value));
        //    sliderpressed = false;
        //}

        //void progressbar_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        //{
        //    if (!sliderpressed)
        //    {
        //        SendMessage(AppConstants.Position, TimeSpan.FromSeconds(e.NewValue));
        //    }
        //}

        //private void videoMediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        //{
        //    // get HRESULT from event args 
        //    string hr = GetHresultFromErrorMessage(e);

        //    // Handle media failed event appropriately 
        //}

        //private string GetHresultFromErrorMessage(ExceptionRoutedEventArgs e)
        //{
        //    String hr = String.Empty;
        //    String token = "HRESULT - ";
        //    const int hrLength = 10;     // eg "0xFFFFFFFF"

        //    int tokenPos = e.ErrorMessage.IndexOf(token, StringComparison.Ordinal);
        //    if (tokenPos != -1)
        //    {
        //        hr = e.ErrorMessage.Substring(tokenPos + token.Length, hrLength);
        //    }

        //    return hr;
        //}

        //private void MainPage_Loaded(object sender, RoutedEventArgs e)
        //{

        //    PointerEventHandler pointerpressedhandler = new PointerEventHandler(slider_PointerEntered);
        //    progressbar.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);

        //    PointerEventHandler pointerreleasedhandler = new PointerEventHandler(slider_PointerCaptureLost);
        //    progressbar.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);
        //}

        //#endregion


        private void SendMessage(string constants)
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
    }
}