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
using System.Threading;

namespace NextPlayerDataLayer.Services
{
    class TrackInfo
    {
        public NowPlayingSong song { get; set; }
        public string timestamp { get; set; }
    }

    public sealed class NowPlayingManager
    {
        private MediaPlayer mediaPlayer;

        TimeSpan startPosition;
        private DateTime songsStart;
        private TimeSpan songPlayed;

        private bool paused;
        private bool isFirst;

        private Playlist playlist;

        public NowPlayingManager()
        {
            playlist = new Playlist();

            startPosition = TimeSpan.Zero;
            songPlayed = TimeSpan.Zero;
            songsStart = DateTime.MinValue;

            paused = false;
            isFirst = true;

            mediaPlayer = BackgroundMediaPlayer.Current;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            //mediaPlayer.CurrentStateChanged += mediaPlayer_CurrentStateChanged;
            mediaPlayer.MediaFailed += mediaPlayer_MediaFailed;
        }

        private async Task LoadFile(string path)
        {
            try
            {
                NowPlayingSong song = playlist.GetCurrentSong();
                //if (song == null)
                //{
                //    //exception
                //    throw new Exception("end of playlist");
                //}
                StorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
                mediaPlayer.AutoPlay = false;
                mediaPlayer.SetFileSource(file);
            }
            catch (Exception e)
            {
                //open default empty song
                if (!paused)
                {
                    Pause();
                }
                //ValueSet message = new ValueSet();
                //message.Add(AppConstants.ShutdownBGPlayer, "");
                //BackgroundMediaPlayer.SendMessageToBackground(message);

                //if (currentSongIndex >= 0 && currentSongIndex < songList.Count)
                //{
                //    if (!paused)
                //    {
                //        Pause();
                //    }
                //    Diagnostics.Logger.SaveBG("NPManager LoadSong() index OK" + "\n" + e.Message);
                //    Diagnostics.Logger.SaveToFileBG();
                //}
                //else
                //{
                //    Diagnostics.Logger.SaveBG("NPManager LoadSong() index not OK" + "\n" + e.Message);
                //    Diagnostics.Logger.SaveToFileBG();

                //    ValueSet message = new ValueSet();
                //    message.Add(AppConstants.ShutdownBGPlayer, "");
                //    BackgroundMediaPlayer.SendMessageToBackground(message);
                //}
            }

        }

        public async Task PlaySong(int index)
        {
            if (!isFirst)
            {
               StopSongEvent();
            }
            else
            {
                isFirst = false;
            }
            playlist.ChangeSong(index);
            paused = false;
            await LoadFile(playlist.GetCurrentSong().Path);
        }

        public async Task ResumePlayback()
        {
            if (mediaPlayer.CurrentState == MediaPlayerState.Playing || mediaPlayer.CurrentState == MediaPlayerState.Paused)
            {
                SendPosition();
                return;
            }
            paused = false;
            isFirst = false;
            object position = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.Position);
            if (position != null)
            {
                startPosition = TimeSpan.Parse(position.ToString());
            }
            await LoadFile(playlist.GetCurrentSong().Path);
        }

        public void Play()
        {
            mediaPlayer.Play();
            paused = false;
            songsStart = DateTime.Now;
        }
        
        public void Pause()
        {
            mediaPlayer.Pause();
            paused = true;
            songPlayed = DateTime.Now - songsStart;
        }
        bool test = false;
        public async Task Next(bool userchoice = true)
        {
            StopSongEvent();
            if (playlist.NextSong(userchoice) == null)
            {
                paused = true;
                return;
            }
            await LoadFile(playlist.GetCurrentSong().Path);
            if (!userchoice)
            {
                ValueSet message = new ValueSet();
                message.Add(AppConstants.UpdateUVC, null);
                BackgroundMediaPlayer.SendMessageToBackground(message);
            }
            SendIndex();
        }

        public async Task Previous()
        {
            if (mediaPlayer.Position > TimeSpan.FromSeconds(5))
            {
                mediaPlayer.Position = TimeSpan.Zero;
            }
            else
            {
                StopSongEvent();
                playlist.PreviousSong();
                await LoadFile(playlist.GetCurrentSong().Path);
                SendIndex();
            }
        }

        public void LoadPlaylist()
        {
            playlist.LoadSongsFromDB();
        }

        private void StopSongEvent()
        {
            UpdateSongStatistics();
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmLogin).ToString() != "" && BackgroundMediaPlayer.Current.NaturalDuration != TimeSpan.Zero)
            {
                ScrobbleTrack();
            }
        }

        private void SendIndex()
        {
            ValueSet message = new ValueSet();
            message.Add(AppConstants.SongIndex, playlist.CurrentIndex.ToString());
            BackgroundMediaPlayer.SendMessageToForeground(message);
        }

        private void SendPosition()
        {
            ValueSet message = new ValueSet();
            message.Add(AppConstants.Position, mediaPlayer.Position.ToString());
            BackgroundMediaPlayer.SendMessageToForeground(message);
        }

        public void CompleteUpdate()
        {
            //SendIndex();
            ValueSet message = new ValueSet();
            message.Add(AppConstants.MediaOpened, BackgroundMediaPlayer.Current.NaturalDuration);
            BackgroundMediaPlayer.SendMessageToForeground(message);
            SendPosition();
        }

        private void ScrobbleTrack()
        {
            if (!paused)
            {
                try
                {
                    songPlayed += DateTime.Now - songsStart;
                }
                catch(Exception ex)
                {
                    Diagnostics.Logger.SaveBG("Scrobble !paused" + Environment.NewLine + ex.Data + Environment.NewLine + ex.Message);
                    Diagnostics.Logger.SaveToFileBG();
                }
            }
            if (songPlayed.TotalSeconds >= BackgroundMediaPlayer.Current.NaturalDuration.TotalSeconds * 0.5 || songPlayed.TotalSeconds >= 4 * 60)
            {
                int seconds = 0;
                try
                {
                    DateTime start = DateTime.UtcNow - songPlayed;
                    seconds = (int)start.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                }
                catch(Exception ex)
                {
                    Diagnostics.Logger.SaveBG("Scrobble paused" + Environment.NewLine + ex.Data + Environment.NewLine + ex.Message);
                    Diagnostics.Logger.SaveToFileBG();
                    return;
                }
                string artist = playlist.GetCurrentSong().Artist;
                string track = playlist.GetCurrentSong().Title;
                string timestamp = seconds.ToString();
                TrackScrobble scrobble = new TrackScrobble()
                {
                    Artist = artist,
                    Track = track,
                    Timestamp = timestamp
                };
                SendScrobble(scrobble);
            }
        }

        private async Task SendScrobble(TrackScrobble scrobble)
        {
            await Task.Run(() => LastFmManager.Current.TrackScroblle(new List<TrackScrobble>() { scrobble }));
        }

        private void ScrobbleNowPlaying()
        {
            if ((bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmSendNP))
            {
                string artist = playlist.GetCurrentSong().Artist;
                string track = playlist.GetCurrentSong().Title;
                SendNowPlayingScrobble(artist, track);
            }
        }

        private async Task SendNowPlayingScrobble(string artist, string track)
        {
            await Task.Run(() => LastFmManager.Current.TrackUpdateNowPlaying(artist, track));
        }

        public void UpdateSongStatistics()
        {
            if (playlist.GetCurrentSong().SongId > 0 && BackgroundMediaPlayer.Current.Position.TotalSeconds >= 5.0)
            {
                DatabaseManager.UpdateSongStatistics(playlist.GetCurrentSong().SongId);
            }
        }

        void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            // wait for media to be ready
            ValueSet message = new ValueSet();
            message.Add(AppConstants.MediaOpened, "");
            BackgroundMediaPlayer.SendMessageToForeground(message);
            songPlayed = TimeSpan.Zero;
            if (!paused)
            {
                sender.Play();
                songsStart = DateTime.Now;
                ScrobbleNowPlaying();
                if (!startPosition.Equals(TimeSpan.Zero))
                {
                    sender.Position = startPosition;
                    startPosition = TimeSpan.Zero;
                }
            }
            else
            {
                songsStart = DateTime.MinValue;
            }
        }

        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            Next(false);
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

        public void ChangeRepeat()
        {
            playlist.ChangeRepeat();
        }

        public void ChangeShuffle()
        {
            playlist.ChangeShuffle();
        }

        public string GetArtist()
        {
            return playlist.GetCurrentSong().Artist;
        }

        public string GetTitle()
        {
            return playlist.GetCurrentSong().Title;
        }

        public void ChangeRate(int percent)
        {
            double rate = percent / 100.0;
            mediaPlayer.PlaybackRate = rate;
        }
    }

    public sealed class NowPlayingManager2
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

        private DateTime songsStart;
        private TimeSpan songPlayed;

        public NowPlayingManager2()
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

            songPlayed = TimeSpan.Zero;
            songsStart = DateTime.Now;

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
                        Pause();
                    }
                    Diagnostics.Logger.SaveBG("NPManager LoadSong() index OK" + "\n" + e.Message);
                    Diagnostics.Logger.SaveToFileBG();
                }
                else
                {
                    Diagnostics.Logger.SaveBG("NPManager LoadSong() index not OK" + "\n" + e.Message);
                    Diagnostics.Logger.SaveToFileBG();

                    ValueSet message = new ValueSet();
                    message.Add(AppConstants.ShutdownBGPlayer, "");
                    BackgroundMediaPlayer.SendMessageToBackground(message);
                }
            }
            
        }
        
        public void StartPlaying(int index)
        {
            ScrobbleTrack();
            UpdateSongStatistics();
            paused = false;
            currentSongIndex = index;
            LoadSong();
           // ScrobbleNowPlaying();
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
            ApplicationSettingsHelper.ReadSettingsValue(AppConstants.SongId);
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
            if (mediaPlayer.Position.Equals(TimeSpan.Zero))
            {
                ScrobbleNowPlaying();
            }
            //SendPosition();
            songsStart = DateTime.Now;
        }

        public void Pause()
        {
            paused = true;
            mediaPlayer.Pause();
            songPlayed += DateTime.Now - songsStart;
        }

        public void Next()
        {
            UpdateSongStatistics();
            ScrobbleTrack();
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
            ScrobbleTrack();
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

        private async Task ScrobbleTrack()
        {
            if (!paused)
            {
                songPlayed += DateTime.Now - songsStart;
            }
            if (songPlayed.TotalSeconds >= BackgroundMediaPlayer.Current.NaturalDuration.TotalSeconds * 0.5 || songPlayed.TotalSeconds >= 4 * 60)
            {
                DateTime start = DateTime.UtcNow - songPlayed;
                int seconds = (int)start.Subtract(new DateTime(1970,1,1)).TotalSeconds;
                await LastFmManager.Current.TrackScroblle(new List<TrackScrobble>() { new TrackScrobble() {
                        Artist = GetArtist(),
                        Track = GetTitle(),
                        Timestamp = seconds.ToString()
                    } });
            }
        }

        private async Task ScrobbleNowPlaying()
        {
            if ((bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmSendNP))
            {
                await LastFmManager.Current.TrackUpdateNowPlaying(GetArtist(), GetTitle());
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
            //Diagnostics.Logger.SaveBG("BG media opened");
            //Diagnostics.Logger.SaveToFileBG();

            // wait for media to be ready
            ValueSet message = new ValueSet();
            message.Add(AppConstants.MediaOpened,"");
            BackgroundMediaPlayer.SendMessageToForeground(message);
            songPlayed = TimeSpan.Zero;
            if (!paused)
            {
                sender.Play();
                songsStart = DateTime.Now;
                ScrobbleNowPlaying();
                if (!startPosition.Equals(TimeSpan.Zero))
                {
                    sender.Position = startPosition;
                    startPosition = TimeSpan.Zero;
                }
            }
        }

        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            //Diagnostics.Logger.SaveBG("BG NP ended");
            //Diagnostics.Logger.SaveToFileBG();

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
                        ScrobbleTrack();
                        ValueSet message = new ValueSet();
                        message.Add(AppConstants.StartPlayback, 0);
                        BackgroundMediaPlayer.SendMessageToBackground(message);
                        //StartPlaying(0);
                        currentSongIndex = 0;//trzeba ustawic wczesniej, poniewaz SendIndex() moze byc wykonane przed StartPlaying()
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

    class Playlist
    {
        private List<NowPlayingSong> playlist;
        private int currentIndex;
        private int previousIndex; // used in shuffle mode
        public int CurrentIndex { get { return currentIndex; } }
        public int SongsCount { get { return playlist.Count; } }
        private bool shuffle;
        private Queue<int> lastPlayed;
        private int maxQueueSize;
        private RepeatEnum repeat;

        private bool isSongRepeated;
        private bool isPlaylistRepeated;

        public Playlist()
        {
            lastPlayed = new Queue<int>();
            LoadSongsFromDB();
            previousIndex = -1;
            shuffle = Shuffle.CurrentState();
            repeat = Repeat.CurrentState();
            isPlaylistRepeated = false;
            isSongRepeated = false;
        }

        public Playlist(int index, bool shuffle, RepeatEnum repeat)
        {
            lastPlayed = new Queue<int>();
            LoadSongsFromDB();
            currentIndex = index;
            previousIndex = -1;
            this.shuffle = shuffle;
            this.repeat = repeat;
            isPlaylistRepeated = false;
            isSongRepeated = false;
        }

        public bool IsFirst()
        {
            return currentIndex == 0;
        }

        public bool IsLast()
        {
            return currentIndex == playlist.Count - 1;
        }

        //zwraca null, jesli nie ma nastepnego utworu do zagrania(np. jest koniec playlisty)
        public NowPlayingSong NextSong(bool userChoice)
        {
            NowPlayingSong song;
            bool stop = false;
            previousIndex = currentIndex;

            if (repeat == RepeatEnum.NoRepeat)
            {
                if (shuffle)
                {
                    currentIndex = GetRandomIndex();
                }
                else
                {
                    if (IsLast())
                    {
                        if (userChoice)
                        {
                            currentIndex = 0;
                        }
                        else
                        {
                            stop = true;
                        }
                    }
                    else
                    {
                        currentIndex++;
                    }
                }
            }
            if (repeat == RepeatEnum.RepeatOnce)
            {
                if (isSongRepeated)
                {
                    isSongRepeated = false;
                    if (shuffle)
                    {
                        currentIndex = GetRandomIndex();
                    }
                    else
                    {
                        if (IsLast())
                        {
                            if (userChoice)
                            {
                                currentIndex = 0;
                            }
                            else
                            {
                                stop = true;
                            }
                        }
                        else
                        {
                            currentIndex++;
                        }
                    }
                }
                else
                {
                    if (userChoice)
                    {
                        if (shuffle)
                        {
                            currentIndex = GetRandomIndex();
                        }
                        else
                        {
                            if (IsLast())
                            {
                                if (userChoice)
                                {
                                    currentIndex = 0;
                                }
                                else
                                {
                                    stop = true;
                                }
                            }
                            else
                            {
                                currentIndex++;
                            }
                        }
                    }
                    else
                    {
                        isSongRepeated = true;
                    }
                }
            }
            else if (repeat == RepeatEnum.RepeatPlaylist)
            {
                if (shuffle)
                {
                    currentIndex = GetRandomIndex();
                }
                else
                {
                    if (isPlaylistRepeated)
                    {
                        if (IsLast())
                        {
                            if (userChoice)
                            {
                                currentIndex = 0;
                            }
                            else
                            {
                                currentIndex = 0;
                                //stop = true;
                            }
                        }
                        else
                        {
                            currentIndex++;
                        }
                    }
                    else
                    {
                        if (IsLast())
                        {
                            currentIndex = 0;
                            isPlaylistRepeated = true;
                        }
                        else
                        {
                            currentIndex++;
                        }
                    }
                }
            }
            if (stop)
            {
                return null;
            }

            ApplicationSettingsHelper.SaveSongIndex(currentIndex);
            song = GetCurrentSong();
            return song;
        }

        public NowPlayingSong PreviousSong()
        {
            NowPlayingSong song;

            isSongRepeated = false;
            if (shuffle)
            {
                if (previousIndex == -1)
                {
                    currentIndex = GetRandomIndex();
                }
                else
                {
                    currentIndex = previousIndex;
                    previousIndex = -1;
                }
            }
            else
            {
                if (IsFirst())
                {
                    currentIndex = playlist.Count - 1;
                }
                else
                {
                    currentIndex--;
                }
            }
            ApplicationSettingsHelper.SaveSongIndex(currentIndex);
            song = GetCurrentSong();
            return song;
        }

        public void ChangeShuffle()
        {
            shuffle = !shuffle;
            if (!shuffle)
            {
                lastPlayed.Clear();
            }
        }

        public void ChangeRepeat()
        {
            repeat = Repeat.CurrentState();
            isPlaylistRepeated = false;
            isSongRepeated = false;
        }

        public void LoadSongsFromDB()
        {
            playlist = DatabaseManager.SelectAllSongsFromNowPlaying();
            currentIndex = ApplicationSettingsHelper.ReadSongIndex();
            if (currentIndex >= playlist.Count)
            {
                currentIndex = playlist.Count - 1;
            }
            //if (currentIndex < 0)
            //{
            //    currentIndex = 0;
            //}
            lastPlayed.Clear();
            if (playlist.Count < 20)
            {
                maxQueueSize = playlist.Count;
            }
            else if (playlist.Count > 60)
            {
                maxQueueSize = 15;
            }
            else
            {
                maxQueueSize = 10 + (playlist.Count / 4);
            }
        }

        private int GetRandomIndex()
        {
            if (playlist.Count == 1)
            {
                return 0;
            }
            Random rnd = new Random();
            int r = rnd.Next(playlist.Count);
            while (r == currentIndex || (playlist.Count > 5 && lastPlayed.Contains(r)))
            {
                r = rnd.Next(playlist.Count);
            }
            if(maxQueueSize == playlist.Count && lastPlayed.Count == maxQueueSize - 1)
            {
                lastPlayed.Clear();
            }
            if (lastPlayed.Count == maxQueueSize)
            {
                lastPlayed.Dequeue();
            }
            lastPlayed.Enqueue(r);
            return r;
        }

        public NowPlayingSong GetCurrentSong()
        {
            if (currentIndex < playlist.Count && currentIndex >= 0)
            {
                return playlist.ElementAt(currentIndex);
            }
            else return new NowPlayingSong() { Artist = "-", Title = "-", Path = "", Position = -1, SongId = -1 };
        }

        public NowPlayingSong ChangeSong(int index)
        {
            previousIndex = currentIndex;
            currentIndex = index;
            isSongRepeated = false;
            ApplicationSettingsHelper.SaveSongIndex(currentIndex);
            return GetCurrentSong();
        }
    }

    
}
