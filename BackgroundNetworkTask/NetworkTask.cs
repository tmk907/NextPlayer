using Windows.ApplicationModel.Background;

namespace BackgroundNetworkTask
{
    public sealed class NetworkTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral = null;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
            await NextPlayerDataLayer.Services.LastFmManager.Current.SendCachedScrobbles();
            _deferral.Complete();
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _deferral.Complete();
        }
    }
}
