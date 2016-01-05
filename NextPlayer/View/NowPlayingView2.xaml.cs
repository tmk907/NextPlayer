using GalaSoft.MvvmLight.Threading;
using NextPlayer.Common;
using NextPlayer.ViewModel;
using NextPlayerDataLayer.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace NextPlayer.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NowPlayingView2 : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        NowPlayingViewModel viewModel;
        public NowPlayingView2()
        {
            this.InitializeComponent();

            //480x800 120 %
            //768x1280 200 %
            //720x1280 180 %
            //540x960 120 %
            //1080x1920 240 %

            //720x1280 140 %
            //1080x1920 220 %
            var a = Window.Current.Bounds.Height;
            var b = Window.Current.Content.RenderSize.Height;
            var c = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            viewModel = (NowPlayingViewModel)DataContext;
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

        #region Slider 
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            PointerEventHandler pointerpressedhandler = new PointerEventHandler(slider_PointerEntered);
            progressbar.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);

            PointerEventHandler pointerreleasedhandler = new PointerEventHandler(slider_PointerCaptureLost);
            progressbar.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);
        }

        void slider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            NowPlayingViewModel viewModel = (NowPlayingViewModel)DataContext;
            viewModel.sliderpressed = true;
        }

        void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            NowPlayingViewModel viewModel = (NowPlayingViewModel)DataContext;
            viewModel.sliderpressed = false;
            viewModel.SendMessage(AppConstants.Position, TimeSpan.FromSeconds(progressbar.Value));
        }

        void progressbar_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            NowPlayingViewModel viewModel = (NowPlayingViewModel)DataContext;
            if (!viewModel.sliderpressed)
            {
                viewModel.SendMessage(AppConstants.Position, TimeSpan.FromSeconds(e.NewValue));
            }
        }
        #endregion
        #region ImageEvents
        private double x, y;

        private void Image_Pressed(object sender, PointerRoutedEventArgs e)
        {
            var a = e.GetCurrentPoint(null);
            x = a.Position.X;
            y = a.Position.Y;
        }

        private void Image_Released(object sender, PointerRoutedEventArgs e)
        {

        }

        private void Image_Exited(object sender, PointerRoutedEventArgs e)
        {
            var a = e.GetCurrentPoint(null);
            if (Math.Abs(x - a.Position.X) > 50)
            {
                if (x - a.Position.X > 0) viewModel.Next();
                else viewModel.Previous();
            }
        }

        private void TextBlock_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            PlaybackRateSlider.Value = 100.0;
        }

        private void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            viewModel.RatingControlVisibility = !viewModel.RatingControlVisibility;
        }

        private void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            viewModel.Play();
        }
        #endregion
    }

    public class SizeNotifyPanel : ContentPresenter
    {
        public static DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(Size), typeof(SizeNotifyPanel), null);

        public Size Size
        {
            get { return (Size)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public SizeNotifyPanel()
        {
            SizeChanged += (s, e) => Size = e.NewSize;
        }
    }
}
