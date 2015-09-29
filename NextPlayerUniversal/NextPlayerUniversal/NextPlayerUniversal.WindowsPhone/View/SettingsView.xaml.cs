using NextPlayerUniversal.Common;
using NextPlayerUniversal.ViewModel;
using NextPlayerUniversal.Constants;
using NextPlayerUniversal.Helpers;
using NextPlayerUniversal.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Media.Playback;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace NextPlayerUniversal.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsView : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        bool isTimerOn;
        bool loading;

        public SettingsView()
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
            //string s = Library.Current.Read();
            //NextPlayerUniversal.Diagnostics.Logger.Save("settings"+s);
            //NextPlayerUniversal.Diagnostics.Logger.SaveToFile();
            var a = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.MediaScan);
            if (a != null)
            {
                DisableControls();
            }
            var d = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.TimerTime);
            
            var t = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.TimerOn);
            if (t == null)
            {
                isTimerOn = false;
            }
            else
            {
                isTimerOn = (bool)t;
            }
            
            if (isTimerOn)
            {
                timerPicker.IsEnabled = true;
                timerToggleSwitch.IsOn = true;

                if (d != null)
                {
                    loading = true;
                    timerPicker.Time = TimeSpan.FromTicks((long)d);
                }
            }
            else
            {
                timerPicker.IsEnabled = false;
                timerToggleSwitch.IsOn = false;
            }
            string isTr = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.TileAppTransparent) as string;
            if (isTr == "yes")
            {
                transparentToggleSwitch.IsOn = true;
            }
            var navigableViewModel = this.DataContext as INavigable;
            if (navigableViewModel != null)
                navigableViewModel.Activate(e.NavigationParameter, e.PageState);
            var s = Windows.Media.Devices.MediaDevice.GetAudioRenderSelector();
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

        private void UpdateLibrary_Click(object sender, RoutedEventArgs e)
        {
            UpdateLibrary();
        }

        private async Task UpdateLibrary()
        {
            DisableControls();
            ProgressRing2.IsActive = true;
            ProgressRing2.Visibility = Visibility.Visible;
            Count2.Text = "";
            Count2.Visibility = Visibility.Visible;
            WaitFewMinutes.Visibility = Visibility.Visible;

            var progressIndicator = new Progress<int>(ReportProgressUpdate);
            await MediaImport.ImportAndUpdateDatabase(progressIndicator);

            WaitFewMinutes.Visibility = Visibility.Collapsed;
            Count2.Visibility = Visibility.Collapsed;
            ProgressRing2.IsActive = false;
            ProgressRing2.Visibility = Visibility.Collapsed;
            EnableControls();          
        }

        private void DisableControls()
        {
            UpdateMediaButton.IsEnabled = false;
        }

        private void EnableControls()
        {
            UpdateMediaButton.IsEnabled = true;
        }

        void ReportProgressUpdate(int value)
        {
            Count2.Text = value.ToString();
        }

        private void Rate_Click(object sender, RoutedEventArgs e)
        {
            Rate();
        }

        private async void Rate()
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            settings.Values[AppConstants.IsReviewed] = -1;
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + AppConstants.AppId)); 
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (((ToggleSwitch)sender).IsOn)
            {
                timerPicker.IsEnabled = true;
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerOn, true);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerTime, timerPicker.Time.Ticks);
                SendMessage(AppConstants.SetTimer);
            }
            else
            {
                timerPicker.IsEnabled = false;
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerOn, false);
                SendMessage(AppConstants.CancelTimer);
            }
        }

        private void TimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (loading)
            {
                loading = false;
            }
            else
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerTime, timerPicker.Time.Ticks);
                TimeSpan t1 = TimeSpan.FromHours(DateTime.Now.Hour) + TimeSpan.FromMinutes(DateTime.Now.Minute);
                long l1 = t1.Ticks;
                long l2 = timerPicker.Time.Ticks;
                TimeSpan t2 = TimeSpan.FromTicks(l2 - l1);
                if (t2 <= TimeSpan.Zero) return;
                else
                {
                    SendMessage(AppConstants.SetTimer);
                }
            }
        }

        private void SendMessage(string s)
        {
            object value = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.BackgroundTaskState);
            if (value == null)
            {
                return;
            }
            else
            {
                bool run = ((String)value).Equals(AppConstants.BackgroundTaskRunning);
                if (run)
                {
                    var msg = new ValueSet();
                    msg.Add(s, "");
                    BackgroundMediaPlayer.SendMessageToBackground(msg);
                }
            }
        }

        private void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerOn, false);
            UpdateUI();
        }

        private async void UpdateUI()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                timerToggleSwitch.IsOn = false;
                timerPicker.IsEnabled = false;
            });
        }

        private async void ShowLog_Click(object sender, RoutedEventArgs e)
        {
            logTB.Text = await NextPlayerUniversal.Diagnostics.Logger.ReadAll();
        }

        private void transparentToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (((ToggleSwitch)sender).IsOn)
            {
                UpdateAppTile(true);
            }
            else
            {
                UpdateAppTile(false);
            }
        }

        public async void UpdateAppTile(bool isTransparent)
        {
            
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Image);
            XmlDocument wideTile = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Image);
            var tileImageAttributes = tileXml.GetElementsByTagName("image");
            var wideImageAttributes = wideTile.GetElementsByTagName("image");
            if (isTransparent)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileAppTransparent, "yes");
                tileImageAttributes[0].Attributes.GetNamedItem("src").NodeValue = "ms-appx:///Assets/AppImages/Logo/LogoTr.png";
                wideImageAttributes[0].Attributes.GetNamedItem("src").NodeValue = "ms-appx:///Assets/AppImages/WideLogo/WideLogoTr.png";
            }
            else
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileAppTransparent, "no");
                tileImageAttributes[0].Attributes.GetNamedItem("src").NodeValue = "ms-appx:///Assets/AppImages/Logo/Logo.png";
                wideImageAttributes[0].Attributes.GetNamedItem("src").NodeValue = "ms-appx:///Assets/AppImages/WideLogo/WideLogo.png";
            }

            IXmlNode node = tileXml.ImportNode(wideTile.GetElementsByTagName("binding").Item(0), true);
            tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node);

            TileNotification tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }
    }
}
