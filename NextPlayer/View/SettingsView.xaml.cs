using NextPlayer.Common;
using NextPlayer.ViewModel;
using NextPlayerDataLayer.Constants;
using NextPlayerDataLayer.Enums;
using NextPlayerDataLayer.Helpers;
using NextPlayerDataLayer.Services;
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
using Windows.UI.Xaml.Shapes;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace NextPlayer.View
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
            if (e.PageState != null && e.PageState.ContainsKey("pivotIndex"))
            {
                PivotSettings.SelectedIndex = (int)e.PageState["pivotIndex"];
            }
           
            //General Settings
            //Library scan
            var a = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.MediaScan);
            if (a != null)
            {
                DisableControls();
            }
            //Timer
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
            //Transparent tile
            string isTr = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.TileAppTransparent) as string;
            if (isTr == "yes")
            {
                transparentToggleSwitch.IsOn = true;
            }

            //Personalize
            //Color accent
            bool isPhoneAccent = (bool) ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IsPhoneAccentSet);
            if (isPhoneAccent)
            {
                phoneAccentToggleSwitch.IsOn = true;
            }
            //Theme
            string appTheme = (string)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AppTheme);
            if (appTheme.Equals(AppThemeEnum.Dark.ToString()))
            {
                RBDark.IsChecked = true;
            }
            else if (appTheme.Equals(AppThemeEnum.Light.ToString()))
            {
                RBLight.IsChecked = true;
            }
            //Background image
            bool isBGImageSet = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IsBGImageSet);
            if (isBGImageSet)
            {
                ShowBGImage_ToggleSwitch.IsOn = true;
                //SelectImage_Button.IsEnabled = true;
            }
            else
            {
                ShowBGImage_ToggleSwitch.IsOn = false;
                //SelectImage_Button.IsEnabled = false;
            }
            //string showCover = (string)ApplicationSettingsHelper.ReadSettingsValue();
            //if (showCover)
            //{
            //    ShowAlbumCover_ToggleSwitch.IsOn = true;
            //}

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
            if (e.PageState.ContainsKey("pivotIndex"))
            {
                e.PageState["pivotIndex"] = PivotSettings.SelectedIndex;
            }
            else
            {
                e.PageState.Add("pivotIndex", PivotSettings.SelectedIndex);
            }
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


        #region Library update
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
        #endregion
        #region Rate app
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
        #endregion
        #region Timer
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
        #endregion
        #region Transparent tile
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
        #endregion
        private async void ShowLog_Click(object sender, RoutedEventArgs e)
        {
            logTB.Text = await NextPlayerDataLayer.Diagnostics.Logger.Read();
        }

        private void gotobl(object sender, RoutedEventArgs e)
        {
            var brush = (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"];
            Windows.UI.Color color = brush.Color;
            color.A = 144;
            ((SolidColorBrush)App.Current.Resources["TransparentColor"]).Color = color;
            App.Current.Resources["UserAccentBrush"] = (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"];
            App.Current.Resources["UserListFontColor"] = new SolidColorBrush(Windows.UI.Colors.White);
        }

        private void BGCover_Toggled(object sender, RoutedEventArgs e)
        {

        }
        #region Color accent
        private void ColorAccent_Toggled(object sender, RoutedEventArgs e)
        {
            if (((ToggleSwitch)sender).IsOn)
            {
                App.Current.Resources["UserAccentBrush"] = ((SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"]);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.IsPhoneAccentSet, true);
            }
            else
            {
                string hexColor = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AppAccent) as string;
                byte a = byte.Parse(hexColor.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                byte r = byte.Parse(hexColor.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(hexColor.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(hexColor.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
                Windows.UI.Color color = Windows.UI.Color.FromArgb(a, r, g, b);
                ((SolidColorBrush)App.Current.Resources["UserAccentBrush"]).Color = color;
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.IsPhoneAccentSet, false);
            }
            NextPlayer.Helpers.StyleHelper.ChangeMainPageButtonsBackground();
        }

        private void Rectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            phoneAccentToggleSwitch.IsOn = false;
            Rectangle rect = sender as Rectangle;
            Windows.UI.Color color = ((SolidColorBrush)rect.Fill).Color;
            ((SolidColorBrush)App.Current.Resources["UserAccentBrush"]).Color = color;
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppAccent, color.ToString());
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.IsPhoneAccentSet, false);
            NextPlayer.Helpers.StyleHelper.ChangeMainPageButtonsBackground();
        }
        #endregion
        #region App theme
        private void RBTheme_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.Name == "RBDark")
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppTheme, AppThemeEnum.Dark.ToString());
            }
            else if (rb.Name == "RBLight")
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppTheme, AppThemeEnum.Light.ToString());
                if ((bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IsBGImageSet))
                {
                    App.Current.Resources["UserListFontColor"] = new SolidColorBrush(Windows.UI.Colors.White);
                }
            }
        }
        #endregion

        #region Background image
        private void BGImage_Toggled(object sender, RoutedEventArgs e)
        {
            if (((ToggleSwitch)sender).IsOn)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.IsBGImageSet, true);
                if (App.Current.RequestedTheme == ApplicationTheme.Light)
                {
                    App.Current.Resources["UserListFontColor"] = new SolidColorBrush(Windows.UI.Colors.White);
                }
                string path = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.BackgroundImagePath) as string;
                if (path != null && path != "")
                {
                    ((ImageBrush)App.Current.Resources["BgImage"]).ImageSource = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                }

                //SelectImage_Button.IsEnabled = true;
            }
            else
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.IsBGImageSet, false);
                //SelectImage_Button.IsEnabled = false; ;

                if (App.Current.RequestedTheme == ApplicationTheme.Light)
                {
                    App.Current.Resources["UserListFontColor"] = new SolidColorBrush(Windows.UI.Colors.Black);
                }
            }
            NextPlayer.Helpers.StyleHelper.ChangeMainPageButtonsBackground();
        }

        private void selectBGImage_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ImageSelection));
        }

        

        #endregion
    }
}
