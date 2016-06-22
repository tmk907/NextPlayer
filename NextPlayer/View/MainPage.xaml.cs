using NextPlayer.Common;
using NextPlayer.ViewModel;
using NextPlayerDataLayer.Constants;
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace NextPlayer.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private NavigationHelper navigationHelper;

        public MainPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var navigableViewModel = this.DataContext as INavigable;
            if (navigableViewModel != null)
                navigableViewModel.Activate(e.NavigationParameter, e.PageState);
            reviewfunction();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            var navigableViewModel = this.DataContext as INavigable;
            if (navigableViewModel != null)
                navigableViewModel.Deactivate(e.PageState);
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            MainPageViewModel viewModel = (MainPageViewModel)DataContext;
            viewModel.Play();
        }

        public async void reviewfunction()
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (!settings.Values.ContainsKey("Win10Version"))
            {
                settings.Values.Add("Win10Version", DateTime.Now.Day);
            }
            else if (settings.Values["Win10Version"].ToString() != "-1")
            {
                int day = (int)settings.Values["Win10Version"];
                if (day == DateTime.Now.Day) return;
                ResourceLoader loader = new ResourceLoader();
                string content = "Try out Next-Player! \nNew music player designed for Windows 10.";
                MessageDialog mydial = new MessageDialog(content);
                mydial.Title = loader.GetString("New music player");
                mydial.Commands.Add(new UICommand(
                    "Download",
                    new UICommandInvokedHandler(this.CommandInvokedHandler_yesclick10)));
                mydial.Commands.Add(new UICommand(
                    "Not now",
                    new UICommandInvokedHandler(this.CommandInvokedHandler_noclick10)));
                await mydial.ShowAsync();
            }
            else
            {
                if (!settings.Values.ContainsKey(AppConstants.IsReviewed))
                {
                    settings.Values.Add(AppConstants.IsReviewed, 0);
                    settings.Values.Add(AppConstants.LastReviewRemind, DateTime.Today.Ticks);
                }
                else
                {
                    int isReviewed = Convert.ToInt32(settings.Values[AppConstants.IsReviewed]);
                    long dateticks = (long)(settings.Values[AppConstants.LastReviewRemind]);
                    TimeSpan elapsed = TimeSpan.FromTicks(DateTime.Today.Ticks - dateticks);
                    if (isReviewed >= 0 && isReviewed < 8 && TimeSpan.FromDays(5) <= elapsed)//!!!!!!!!! <=
                    {
                        settings.Values[AppConstants.LastReviewRemind] = DateTime.Today.Ticks;
                        settings.Values[AppConstants.IsReviewed] = isReviewed++;
                        ResourceLoader loader = new ResourceLoader();

                        MessageDialog mydial = new MessageDialog(loader.GetString("RateAppMsg"));
                        mydial.Title = loader.GetString("RateAppTitle");
                        mydial.Commands.Add(new UICommand(
                            loader.GetString("Yes"),
                            new UICommandInvokedHandler(this.CommandInvokedHandler_yesclick)));
                        mydial.Commands.Add(new UICommand(
                           loader.GetString("Later"),
                           new UICommandInvokedHandler(this.CommandInvokedHandler_noclick)));
                        await mydial.ShowAsync();
                    }
                }
            }
        }

        private void CommandInvokedHandler_noclick(IUICommand command)
        {

        }

        private void CommandInvokedHandler_noclick10(IUICommand command)
        {

        }

        private async void CommandInvokedHandler_yesclick(IUICommand command)
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            settings.Values[AppConstants.IsReviewed] = -1;

            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + AppConstants.AppId));
        }

        private async void CommandInvokedHandler_yesclick10(IUICommand command)
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            settings.Values["Win10Version"] = -1;
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?ProductId=9nblggh67n4f"));
            DiagnosticHelper.TrackEvent("Download Next-Player");
        }
    }
}
