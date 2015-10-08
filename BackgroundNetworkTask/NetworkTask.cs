using NextPlayerDataLayer.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace BackgroundNetworkTask
{
    public sealed class NetworkTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral = null;
        IBackgroundTaskInstance _taskInstance = null;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
            Sender sender = new Sender();
            await sender.Go();
            _deferral.Complete();
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _deferral.Complete();
        }
    }
}
