using NextPlayerDataLayer.Constants;
using System;
using Windows.Foundation.Collections;
using Windows.Media.Playback;

namespace NextPlayerDataLayer.Helpers
{
    public class NPChange
    {
        private static bool IsMyBackgroundTaskRunning
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

        public static void SendMessageNPChanged()
        {
            if (IsMyBackgroundTaskRunning)
            {
                var value = new ValueSet();
                value.Add(AppConstants.NowPlayingListChanged, "");
                BackgroundMediaPlayer.SendMessageToBackground(value);
            }
        }

        //same songs, different order or tags update
        public static void SendMessageRefreshNP(int songId, string title, string artist)
        {
            if (IsMyBackgroundTaskRunning)
            {
                var value = new ValueSet();
                value.Add(AppConstants.NowPlayingListRefresh, songId + "!@#$" + title + "!@#$" + artist);
                BackgroundMediaPlayer.SendMessageToBackground(value);
            }
        }

        public static void SendMessageNPSorted()
        {
            if (IsMyBackgroundTaskRunning)
            {
                var value = new ValueSet();
                value.Add(AppConstants.NowPlayingListSorted, "");
                BackgroundMediaPlayer.SendMessageToBackground(value);
            }
        }
    }
}
