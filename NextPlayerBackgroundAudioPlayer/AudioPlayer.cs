using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Media;
using Windows.Media.Playback;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.Foundation.Collections;
using NextPlayerDataLayer.Constants;
using NextPlayerDataLayer.Helpers;
using NextPlayerDataLayer.Model;
using NextPlayerDataLayer.Services;
using Windows.System.Threading;

namespace NextPlayerBackgroundAudioPlayer
{
    enum ForegroundTaskStatus
    {
        Active,
        Suspended,
        Unknown
    }

    enum BackgroundTaskStatus
    {
        Active,
        Suspended,
        Unknown
    }

    public sealed class AudioPlayer : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;
        private SystemMediaTransportControls systemControls;
        private AutoResetEvent backgroundTaskStarted = new AutoResetEvent(false);
        private bool backgroundTaskStatus = false;
        private ForegroundTaskStatus foregroundTaskStatus = ForegroundTaskStatus.Unknown;

        private NowPlayingManager nowPlayingManager;
        private bool muted = false;

        ThreadPoolTimer timer = null;
        bool timerIsSet = false;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            NextPlayerDataLayer.Diagnostics.Logger.SaveBG("BG Task Started");
            NextPlayerDataLayer.Diagnostics.Logger.SaveToFileBG();

            nowPlayingManager = new NowPlayingManager();

            systemControls = SystemMediaTransportControls.GetForCurrentView();
            systemControls.ButtonPressed += HandleButtonPressed;
            systemControls.PropertyChanged += HandlePropertyChanged;
            systemControls.IsEnabled = true;
            systemControls.IsPauseEnabled = true;
            systemControls.IsPlayEnabled = true;
            systemControls.IsPreviousEnabled = true;
            systemControls.IsNextEnabled = true;

            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
            taskInstance.Task.Completed += HandleTaskCompleted;

            var value = ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.AppState);
            if (value == null)
                foregroundTaskStatus = ForegroundTaskStatus.Unknown;
            else
                foregroundTaskStatus = (ForegroundTaskStatus)Enum.Parse(typeof(ForegroundTaskStatus), value.ToString());

            BackgroundMediaPlayer.Current.CurrentStateChanged += BGCurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromForeground += MessageReceivedFromForeground;

            //taskInstance.Task.Progress += HandleTaskProgress;

            if (foregroundTaskStatus != ForegroundTaskStatus.Suspended)
            {
                ValueSet message = new ValueSet();
                message.Add(AppConstants.BackgroundTaskStarted, "");
                BackgroundMediaPlayer.SendMessageToForeground(message);
            }
           
            backgroundTaskStarted.Set();
            backgroundTaskStatus = true;
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.BackgroundTaskState, AppConstants.BackgroundTaskRunning);
            shutdown = false;
            deferral = taskInstance.GetDeferral();
        }

        private void HandleTaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            NextPlayerDataLayer.Diagnostics.Logger.SaveBG("BG HandleTaskCompleted");
            NextPlayerDataLayer.Diagnostics.Logger.SaveToFileBG();
            //BackgroundMediaPlayer.Shutdown();
            deferral.Complete();
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            try
            {
                BackgroundMediaPlayer.Current.Pause();
                NextPlayerDataLayer.Diagnostics.Logger.SaveBG("BG Before Cancel");
                NextPlayerDataLayer.Diagnostics.Logger.SaveToFileBG();
                BackgroundMediaPlayer.MessageReceivedFromForeground -= MessageReceivedFromForeground;
                //ValueSet message = new ValueSet();
                //message.Add(AppConstants.PlayerClosed, "");
                //BackgroundMediaPlayer.SendMessageToForeground(message);

                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.SongIndex, nowPlayingManager.currentSongIndex);
                if (shutdown)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.Position, TimeSpan.Zero.ToString());
                }
                else
                {
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.Position, BackgroundMediaPlayer.Current.Position.ToString());
                }

                nowPlayingManager.RemoveHandlers();
                nowPlayingManager = null;

                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.BackgroundTaskState, AppConstants.BackgroundTaskCancelled);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppState, Enum.GetName(typeof(ForegroundTaskStatus), foregroundTaskStatus));
                backgroundTaskStatus = false;
                
                //unsubscribe event handlers
                systemControls.ButtonPressed -= HandleButtonPressed;
                systemControls.PropertyChanged -= HandlePropertyChanged;
                if (!shutdown)
                {
                    BackgroundMediaPlayer.Shutdown(); // shutdown media pipeline

                }
            }
            catch (Exception ex)
            {
                NextPlayerDataLayer.Diagnostics.Logger.SaveBG("Audio Player OnCanceled " + "\n" + "Message: " + ex.Message + "\n" + "Link: " + ex.HelpLink);
                NextPlayerDataLayer.Diagnostics.Logger.SaveToFileBG();
            }
            NextPlayerDataLayer.Diagnostics.Logger.SaveBG("BG After Cancel");
            NextPlayerDataLayer.Diagnostics.Logger.SaveToFileBG();
            deferral.Complete(); // signals task completion. 
        }


        private void MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            foreach (string key in e.Data.Keys)
            {
                switch (key.ToLower())
                {
                    case AppConstants.StartPlayback:
                        nowPlayingManager.StartPlaying(Int32.Parse(e.Data.Where(z => z.Key.Equals(key)).FirstOrDefault().Value.ToString()));
                        UpdateUVCOnNewTrack();
                        break;
                    case AppConstants.ResumePlayback:
                        nowPlayingManager.ResumePlayback();
                        UpdateUVCOnNewTrack();
                        break;
                    case AppConstants.SkipNext:
                        Next();
                        break;
                    case AppConstants.SkipPrevious:
                        Previous();
                        break;
                    case AppConstants.Play:
                        Play();
                        break;
                    case AppConstants.Pause:
                        Pause();
                        break;
                    case AppConstants.AppResumed:
                        foregroundTaskStatus = ForegroundTaskStatus.Active;
                        nowPlayingManager.CompleteUpdate();
                        break;
                    case AppConstants.AppSuspended:
                        foregroundTaskStatus = ForegroundTaskStatus.Suspended;
                        //ApplicationSettingsHelper.SaveSettingsValue(Constants.SongId, nowPlayingManager.GetCurrentSongId());
                        break;
                    case AppConstants.NowPlayingListChanged:
                        nowPlayingManager.UpdateNowPlayingList();
                        break;
                    case AppConstants.Repeat:
                        nowPlayingManager.ChangeRepeat();
                        break;
                    case AppConstants.Shuffle:
                        nowPlayingManager.ChangeShuffle();
                        break;
                    case AppConstants.Position:
                        BackgroundMediaPlayer.Current.Position = TimeSpan.Parse(e.Data.Where(z => z.Key.Equals(key)).FirstOrDefault().Value.ToString());
                        break;
                    case AppConstants.NowPlayingListSorted:
                        nowPlayingManager.UpdateNowPlayingList2();
                        break;
                    case AppConstants.SetTimer:
                        SetTimer();
                        break;
                    case AppConstants.CancelTimer:
                        TimerCancel();
                        break;
                    case AppConstants.ShutdownBGPlayer:
                        ShutdownPlayer();
                        //ClearPlayer();
                        break;
                }
            }
        }

        private void HandleButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    if (!backgroundTaskStatus)
                    {
                        bool result = backgroundTaskStarted.WaitOne(2000);
                        if (!result)
                        {
                            NextPlayerDataLayer.Diagnostics.Logger.SaveBG("Audio Player HandleButtonPressed Play" + "\n" + "Background Task didnt initialize in time" + "Button: " + args.Button + "\n" + "Status: " + sender.PlaybackStatus.ToString());
                            NextPlayerDataLayer.Diagnostics.Logger.SaveToFileBG();
                            throw new Exception("Background Task didnt initialize in time");
                        }
                    }
                    Play();

                    break;
                case SystemMediaTransportControlsButton.Pause:
                    try
                    {
                        Pause();
                    }
                    catch (Exception ex)
                    {
                        NextPlayerDataLayer.Diagnostics.Logger.SaveBG("Audio Player HandleButtonPressed Pause" + "\n" + "Message: " + ex.Message + "\n" + "Link: " + ex.HelpLink);
                        NextPlayerDataLayer.Diagnostics.Logger.SaveToFileBG();
                    }
                    break;
                case SystemMediaTransportControlsButton.Next:
                    try
                    {
                        Next();
                    }
                    catch (Exception ex)
                    {
                        NextPlayerDataLayer.Diagnostics.Logger.SaveBG("Audio Player HandleButtonPressed Next" + "\n" + "Message: " + ex.Message + "\n" + "Link: " + ex.HelpLink);
                        NextPlayerDataLayer.Diagnostics.Logger.SaveToFileBG();
                    }
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    try
                    {
                        Previous();
                    }
                    catch (Exception ex)
                    {
                        NextPlayerDataLayer.Diagnostics.Logger.SaveBG("Audio Player HandleButtonPressed Previous" + "\n" + "Message: " + ex.Message + "\n" + "Link: " + ex.HelpLink);
                        NextPlayerDataLayer.Diagnostics.Logger.SaveToFileBG();
                    }
                    break;
                default:
                    break;
            }
        }

        private void HandlePropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            //if (sender.SoundLevel == SoundLevel.Muted)
            //{
            //    Pause();
            //    muted = true;
            //}
            //else if (muted && sender.SoundLevel != SoundLevel.Muted)
            //{
            //    Play();
            //    muted = false;
            //}
        }

        private void BGCurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing)
            {
                systemControls.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
            else if (sender.CurrentState == MediaPlayerState.Paused)
            {
                systemControls.PlaybackStatus = MediaPlaybackStatus.Paused;
            }
        }

        private void Play()
        {
            nowPlayingManager.Play();
        }

        private void Pause()
        {
            nowPlayingManager.Pause();
        }

        private void Next()
        {
            nowPlayingManager.Next();
            UpdateUVCOnNewTrack();
        }

        private void Previous()
        {
            nowPlayingManager.Previous();
            UpdateUVCOnNewTrack();
        }

        private void UpdateUVCOnNewTrack()
        {
            //if (!systemControls.IsEnabled) systemControls.IsEnabled = true;
            systemControls.DisplayUpdater.Type = MediaPlaybackType.Music;
            systemControls.DisplayUpdater.MusicProperties.Title = nowPlayingManager.GetTitle();
            systemControls.DisplayUpdater.MusicProperties.Artist = nowPlayingManager.GetArtist();
            systemControls.DisplayUpdater.Update();
        }
        bool shutdown;
        private void ShutdownPlayer()
        {
            //Pause();
            //NextPlayerDataLayer.Diagnostics.Logger.SaveBG("BG Shutdown");
            //NextPlayerDataLayer.Diagnostics.Logger.SaveToFileBG();
            shutdown = true;
            BackgroundMediaPlayer.Shutdown();
        }

        private void ClearPlayer()
        {
            Pause();
            BackgroundMediaPlayer.Current.Position = TimeSpan.Zero;
            ApplicationSettingsHelper.SaveSongIndex(nowPlayingManager.currentSongIndex);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.Position, TimeSpan.Zero.ToString());
            systemControls.IsEnabled = false;
        }

        private void SetTimer()
        {
            var t = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.TimerTime);
            long tt = 0;
            if (t != null)
            {
                tt = (long)t;
            }

            TimeSpan currentTime = TimeSpan.FromHours(DateTime.Now.Hour) + TimeSpan.FromMinutes(DateTime.Now.Minute) + TimeSpan.FromSeconds(DateTime.Now.Second);
            long currentTimeTicks = currentTime.Ticks;

            TimeSpan delay = TimeSpan.FromTicks(tt - currentTimeTicks);
            if (delay > TimeSpan.Zero)
            {
                if (timerIsSet)
                {
                    TimerCancel();
                }
                timer = ThreadPoolTimer.CreateTimer(new TimerElapsedHandler(TimerCallback), delay);
                timerIsSet = true;
            }
        }

        private void TimerCallback(ThreadPoolTimer timer)
        {
            //NextPlayerDataLayer.Diagnostics.Logger.SaveBG("BG Timer Cancel");
            ShutdownPlayer();
            //ClearPlayer();
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerOn, false);
            TimerCancel();
        }

        private void TimerCancel()
        {
            timerIsSet = false;
            if (timer != null)
            {
                timer.Cancel();
            }
        }
    }
}
