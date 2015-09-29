using NextPlayerUniversal.Constants;
using NextPlayerUniversal.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;

namespace NextPlayerUniversalWP.Helpers
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
