using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextPlayerDataLayer.Enums;
using NextPlayerDataLayer.Model;
using System.Collections.ObjectModel;
using Windows.Media.Playback;
using Windows.Storage;
using NextPlayerDataLayer.Helpers;
using NextPlayerDataLayer.Constants;
using Windows.Foundation.Collections;

namespace NextPlayerDataLayer.Services
{
    
    public sealed class NowPlayingManager
    {
        private List<NowPlayingSong> songList;
        public int currentSongIndex;
        private bool paused;
        private bool isShuffleOn;
        private bool isSongRepeated = false;
        private bool isPlaylistRepeated = false;
        private RepeatEnum repeat;
        private MediaPlayer mediaPlayer;
        TimeSpan startPosition = TimeSpan.FromSeconds(0);
        private List<int> shuffleSongIndexes = new List<int>();
        int prevSongIndex = -1;
        private int currentSongId = -1;


        public NowPlayingManager()
        {
            songList = DatabaseManager.SelectAllSongsFromNowPlaying();

            object currentIndex = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.SongIndex);
            object prevIndex = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.PrevSongIndex);
            if (currentIndex == null) currentSongIndex = 0;
            else currentSongIndex = Int32.Parse(currentIndex.ToString());
            startPosition = TimeSpan.Zero;
            //if (prevIndex != null && Int32.Parse(prevIndex.ToString()) == currentSongIndex)
            //{
            //    object value2 = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.Position);
            //    if (value2 != null) startPosition = TimeSpan.Parse(value2.ToString());
            //}

            //Shuffle
            isShuffleOn = Shuffle.CurrentState();

            //Repeat
            repeat = Repeat.CurrentState();

            paused = false;

            mediaPlayer = BackgroundMediaPlayer.Current;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            //mediaPlayer.CurrentStateChanged += mediaPlayer_CurrentStateChanged;
            mediaPlayer.MediaFailed += mediaPlayer_MediaFailed;
        }


        public bool IsPrevious()
        {
            return currentSongIndex != 0;
        }

        public bool IsNext()
        {
            return currentSongIndex != songList.Count() - 1;
        }

        private async Task LoadSong()
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(songList.ElementAt<NowPlayingSong>(currentSongIndex).Path);
                mediaPlayer.AutoPlay = false;
                mediaPlayer.SetFileSource(file);
                currentSongId = songList.ElementAt(currentSongIndex).SongId;
            }
            catch (Exception e)
            {
                if (currentSongIndex >= 0 && currentSongIndex < songList.Count)
                {
                    if (!paused)
                    {
                        SendSkipNext();
                    }
                }
                else
                {
                    ValueSet message = new ValueSet();
                    message.Add(AppConstants.ShutdownBGPlayer, "");
                    BackgroundMediaPlayer.SendMessageToBackground(message);
                }
            }
            
        }
        
        public void StartPlaying(int index)
        {
            UpdateSongStatistics();
            paused = false;
            currentSongIndex = index;
            LoadSong();
        }

        public void ResumePlayback()
        {
            if (mediaPlayer.CurrentState == MediaPlayerState.Playing || mediaPlayer.CurrentState == MediaPlayerState.Paused)
            {
                SendPosition();
                return;
            }
            paused = false;
            object value2 = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.Position);
            if (value2 != null)
            {
                startPosition = TimeSpan.Parse(value2.ToString());
            }
            LoadSong();
        }

        public void Play()
        {
            paused = false;
            mediaPlayer.Play();
            //SendPosition();
        }

        public void Pause()
        {
            paused = true;
            mediaPlayer.Pause();
        }

        public void Next()
        {
            UpdateSongStatistics();

            if (isShuffleOn)
            {
                prevSongIndex = currentSongIndex;
                currentSongIndex = GetRandomNumber();
            }
            else currentSongIndex++;
            if (currentSongIndex == songList.Count)
            {
                currentSongIndex--;
                mediaPlayer.Position = TimeSpan.Zero;
                Pause();
            }
            else
            {
                if (paused)
                {
                    LoadSong();
                }
                else
                {
                    LoadSong();
                }
            }
            SendIndex();
            isSongRepeated = false;
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.SongIndex, currentSongIndex);
        }

        public void Previous()
        {
            UpdateSongStatistics();

            if (isShuffleOn)
            {
                //currentSongIndex = shuffleSongIndexes.Last();
                //shuffleSongIndexes.RemoveAt(shuffleSongIndexes.Count - 1);
                if (prevSongIndex == -1) currentSongIndex = GetRandomNumber();
                else
                {
                    currentSongIndex = prevSongIndex;
                    prevSongIndex = -1;
                }
            }
            else currentSongIndex--;
            if (currentSongIndex < 0)
            {
                currentSongIndex++;
                
                mediaPlayer.Position = TimeSpan.Zero;
                Pause();
            }
            else
            {
                if (paused) //dodac currenttimetrack
                {
                    LoadSong();
                }
                else
                {
                    LoadSong();
                }
            }
            SendIndex();
            isSongRepeated = false;
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.SongIndex, currentSongIndex);
        }

        private void SendIndex()
        {
            ValueSet message = new ValueSet();
            message.Add(AppConstants.SongIndex, currentSongIndex.ToString());
            BackgroundMediaPlayer.SendMessageToForeground(message);
        }

        private void SendPosition()
        {
            ValueSet message = new ValueSet();
            message.Add(AppConstants.Position, mediaPlayer.Position.ToString());
            BackgroundMediaPlayer.SendMessageToForeground(message);
        }

        private void SendSkipNext()
        {
            ValueSet message = new ValueSet();
            message.Add(AppConstants.SkipNext, "");
            BackgroundMediaPlayer.SendMessageToBackground(message);
        }

        public void CompleteUpdate()
        {
            SendIndex();
            ValueSet message = new ValueSet();
            message.Add(AppConstants.MediaOpened, BackgroundMediaPlayer.Current.NaturalDuration);
            BackgroundMediaPlayer.SendMessageToForeground(message);
            SendPosition();
        }

        private int GetRandomNumber()
        {
            if (songList.Count == 1)
            {
                return 0;
            }
            Random rnd = new Random();
            int r = rnd.Next(songList.Count);
            while (r == currentSongIndex)
            {
                r = rnd.Next(songList.Count);
            }
            return r;
        }

        public void ChangeShuffle()
        {
            isShuffleOn = !isShuffleOn;
        }

        public void ChangeRepeat()
        {
            repeat = Repeat.CurrentState();
            ResetRepeat();
        }

        private void ResetRepeat()
        {
            isSongRepeated = false;
            isPlaylistRepeated = false;
        }

        public string GetTitle()
        {
            if (currentSongIndex < songList.Count && currentSongIndex >= 0)
            {
                return songList.ElementAt(currentSongIndex).Title;
            }
            else return "-";
        }

        public string GetArtist()
        {
            if (currentSongIndex < songList.Count && currentSongIndex >= 0)
            {
                return songList.ElementAt(currentSongIndex).Artist;
            }
            else return "-";
        }

        public void UpdateNowPlayingList()
        {
            songList = DatabaseManager.SelectAllSongsFromNowPlaying();
            if (currentSongIndex >= songList.Count)
            {
                currentSongIndex = songList.Count - 1;
            }
        }

        public void UpdateNowPlayingList2()
        {
            songList = DatabaseManager.SelectAllSongsFromNowPlaying();
            currentSongIndex = ApplicationSettingsHelper.ReadSongIndex();
            if (currentSongIndex >= songList.Count)
            {
                currentSongIndex = songList.Count - 1;
            }
        }

        public void UpdateSongStatistics()
        {
            if (currentSongId>0 &&  BackgroundMediaPlayer.Current.Position.TotalSeconds >= 5.0)
            {
                DatabaseManager.UpdateSongStatistics(currentSongId);

            }
        }

        #region MediaPlayer Handlers

        void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            NextPlayerDataLayer.Diagnostics.Logger.SaveBG("BG media opened");
            NextPlayerDataLayer.Diagnostics.Logger.SaveToFileBG();

            // wait for media to be ready
            ValueSet message = new ValueSet();
            message.Add(AppConstants.MediaOpened,"");
            BackgroundMediaPlayer.SendMessageToForeground(message);
            if (!paused)
            {
                sender.Play();
                if (!startPosition.Equals(TimeSpan.Zero))
                {
                    sender.Position = startPosition;
                    startPosition = TimeSpan.Zero;
                }
            }
        }

        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            NextPlayerDataLayer.Diagnostics.Logger.SaveBG("BG NP ended");
            NextPlayerDataLayer.Diagnostics.Logger.SaveToFileBG();

            if (repeat.Equals(RepeatEnum.NoRepeat))
            {
                SendSkipNext();
            }
            else if (repeat.Equals(RepeatEnum.RepeatOnce))
            {
                if (isSongRepeated)
                {
                    SendSkipNext();
                }
                else
                {
                    StartPlaying(currentSongIndex);
                }
                isSongRepeated = !isSongRepeated;
            }
            else if (repeat.Equals(RepeatEnum.RepeatPlaylist))
            {
                if (!isShuffleOn && !IsNext())
                {
                    if (isPlaylistRepeated)
                    {
                        SendSkipNext();
                    }
                    else
                    {
                        StartPlaying(0);
                        SendIndex();
                    }
                    isPlaylistRepeated = !isPlaylistRepeated;
                }
                else
                {
                    SendSkipNext();
                }
            }
        }

        private void mediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            //Debug.WriteLine("Failed with error code " + args.ExtendedErrorCode.ToString());
        }

        public void RemoveHandlers()
        {
            mediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
            //mediaPlayer.CurrentStateChanged -= mediaPlayer_CurrentStateChanged;
            mediaPlayer.MediaFailed -= mediaPlayer_MediaFailed;
            mediaPlayer = null;
        }

        //private void mediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        //{

        //    //if (sender.CurrentState == MediaPlayerState.Playing && startPosition != TimeSpan.FromSeconds(0))
        //    //{
        //    //    // if the start position is other than 0, then set it now
        //    //    sender.Position = startPosition;
        //    //    sender.Volume = 1.0;
        //    //    startPosition = TimeSpan.FromSeconds(0);
        //    //    sender.PlaybackMediaMarkers.Clear();
        //    //    paused = false;
        //    //}
        //}
        #endregion

    }
}
