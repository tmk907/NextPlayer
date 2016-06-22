using NextPlayer.Common;
using NextPlayer.ViewModel;
using NextPlayerDataLayer.Model;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace NextPlayer.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlaylistsView : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public PlaylistsView()
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
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
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

        private void newPlainPlaylist_Click(object sender, RoutedEventArgs e)
        {
            playlistNameTextBox.Text = "";
            FlyoutBase.SetAttachedFlyout(this, (FlyoutBase)this.Resources["NewPlaylistFlyout"]);
            FlyoutBase.ShowAttachedFlyout(this);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (playlistNameTextBox.Text == "")
            {
                playlistNameTextBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
            }
            else
            {
                PlaylistsViewModel ViewModel = (PlaylistsViewModel)DataContext;
                ViewModel.AddPlainPlaylist(playlistNameTextBox.Text);
                FlyoutBase.GetAttachedFlyout(this).Hide();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.GetAttachedFlyout(this).Hide();
        }

            private PlaylistItem p;
            private void Delete_Click(object sender, RoutedEventArgs e)
            {
                p = (e.OriginalSource as FrameworkElement).DataContext as PlaylistItem;
            
                FlyoutBase.SetAttachedFlyout(this, (FlyoutBase)this.Resources["DeletePlaylistFlyout"]);
                FlyoutBase.ShowAttachedFlyout(this);
            }

            private void DeleteConfirm_Click(object sender, RoutedEventArgs e)
            {
                PlaylistsViewModel ViewModel = (PlaylistsViewModel)DataContext;
                ViewModel.DeletePlaylist(p);
            
                FlyoutBase.GetAttachedFlyout(this).Hide();
            }

        private void AddToNP_Click(object sender, RoutedEventArgs e)
        {
           PlaylistItem item = (e.OriginalSource as FrameworkElement).DataContext as PlaylistItem;
           PlaylistsViewModel ViewModel = (PlaylistsViewModel)DataContext;
           ViewModel.AddToNowPlaying(item);
        }

        private void Pin_Click(object sender, RoutedEventArgs e)
        {
            PlaylistItem item = (e.OriginalSource as FrameworkElement).DataContext as PlaylistItem;
            PlaylistsViewModel ViewModel = (PlaylistsViewModel)DataContext;
            ViewModel.PinPlaylist(item);
        }

        private void PlayNow_Click(object sender, RoutedEventArgs e)
        {
            PlaylistItem item = (e.OriginalSource as FrameworkElement).DataContext as PlaylistItem;
            PlaylistsViewModel ViewModel = (PlaylistsViewModel)DataContext;
            ViewModel.PlayNow(item);
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            PlaylistItem item = (e.OriginalSource as FrameworkElement).DataContext as PlaylistItem;
            PlaylistsViewModel ViewModel = (PlaylistsViewModel)DataContext;
            ViewModel.Share(item);
        }
    }
}
