using NextPlayerDataLayer.Constants;
using NextPlayerDataLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Media.Playback;
using Windows.System.Threading;

namespace NextPlayerBackgroundAudioTimer
{
    public sealed class BackgroundTimer : IBackgroundTask
    {
        BackgroundTaskCancellationReason _cancelReason = BackgroundTaskCancellationReason.Abort;
        volatile bool _cancelRequested = false;
        BackgroundTaskDeferral _deferral = null;
        ThreadPoolTimer timer = null;
        
        IBackgroundTaskInstance _taskInstance = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);

            _deferral = taskInstance.GetDeferral();

            _taskInstance = taskInstance;

            var t = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.TimerTime);
            long tt = 0;
            if (t != null)
            {
                tt = (long)t;
            }

            TimeSpan t1 = TimeSpan.FromHours(DateTime.Now.Hour) + TimeSpan.FromMinutes(DateTime.Now.Minute);
            long ct = t1.Ticks;
           
            TimeSpan t2 = TimeSpan.FromTicks(tt-ct);
            if (t2 <= TimeSpan.Zero)
            {
                _deferral.Complete();
            }
            else
            {
                TimeSpan delay = TimeSpan.FromMinutes(1);
                timer = ThreadPoolTimer.CreateTimer(new TimerElapsedHandler(TimerCallback), delay);
            }
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _cancelRequested = true;
            _cancelReason = reason;
        }

        private void TimerCallback(ThreadPoolTimer timer)
        {
            if ((_cancelRequested == false))
            {
                BackgroundMediaPlayer.Shutdown();
                
            }
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerOn, false);
            timer.Cancel();

             _deferral.Complete();
        }

    }
}
