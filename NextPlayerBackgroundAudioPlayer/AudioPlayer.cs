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

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            nowPlayingManager = new NowPlayingManager();

            systemControls = SystemMediaTransportControls.GetForCurrentView();
            systemControls.ButtonPressed += HandleButtonPressed;
            systemControls.PropertyChanged += HandlePropertyChanged;
            systemControls.IsEnabled = true;
            systemControls.IsPauseEnabled = true;
            systemControls.IsPlayEnabled = true;
            systemControls.IsPreviousEnabled = true;
            systemControls.IsNextEnabled = true;

            var value = ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.AppState);
            if (value == null)
                foregroundTaskStatus = ForegroundTaskStatus.Unknown;
            else
                foregroundTaskStatus = (ForegroundTaskStatus)Enum.Parse(typeof(ForegroundTaskStatus), value.ToString());


            BackgroundMediaPlayer.Current.CurrentStateChanged += BGCurrentStateChanged;

            BackgroundMediaPlayer.MessageReceivedFromForeground += MessageReceivedFromForeground;

            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
            taskInstance.Task.Completed += HandleTaskCompleted;
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

            deferral = taskInstance.GetDeferral();
        }

        private void HandleTaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            //BackgroundMediaPlayer.Shutdown();
            deferral.Complete();
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            // You get some time here to save your state before process and resources are reclaimed
            //Debug.WriteLine("MyBackgroundAudioTask " + sender.Task.TaskId + " Cancel Requested...");
            try
            {
                BackgroundMediaPlayer.Current.Pause();
                //save state
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.SongIndex, nowPlayingManager.currentSongIndex);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.Position, BackgroundMediaPlayer.Current.Position.ToString());
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.BackgroundTaskState, AppConstants.BackgroundTaskCancelled);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppState, Enum.GetName(typeof(ForegroundTaskStatus), foregroundTaskStatus));
                backgroundTaskStatus = false;
                //unsubscribe event handlers
                
                systemControls.ButtonPressed -= HandleButtonPressed;
                systemControls.PropertyChanged -= HandlePropertyChanged;
                
                //Playlist.TrackChanged -= playList_TrackChanged;

                //clear objects task cancellation can happen uninterrupted
                //playlistManager.ClearPlaylist();
                //playlistManager = null;
                BackgroundMediaPlayer.Shutdown(); // shutdown media pipeline
            }
            catch (Exception ex)
            {

            }
            deferral.Complete(); // signals task completion. 
            //Debug.WriteLine("MyBackgroundAudioTask Cancel complete...");
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
                            throw new Exception("Background Task didnt initialize in time");
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
                        //Debug.WriteLine(ex.ToString());
                    }
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Next();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Previous();
                    break;
                default:
                    break;
            }
        }

        private void HandlePropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            if (sender.SoundLevel == SoundLevel.Muted)
            {
                Pause();
                muted = true;
            }
            else if (muted && sender.SoundLevel != SoundLevel.Muted)
            {
                Play();
                muted = false;
            }
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
            //systemControls.PlaybackStatus = MediaPlaybackStatus.Playing;
            systemControls.DisplayUpdater.Type = MediaPlaybackType.Music;
            systemControls.DisplayUpdater.MusicProperties.Title = nowPlayingManager.GetTitle();
            systemControls.DisplayUpdater.MusicProperties.Artist = nowPlayingManager.GetArtist();
            systemControls.DisplayUpdater.Update();
        }
    }
}
