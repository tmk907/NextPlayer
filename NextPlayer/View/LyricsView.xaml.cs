using NextPlayer.Common;
using NextPlayer.Converters;
using NextPlayerDataLayer.Helpers;
using NextPlayerDataLayer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
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
    public sealed partial class LyricsView : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        string address;
        string artist;
        string title;
        int songId;
        string lyrics;
        bool original;
        Windows.ApplicationModel.Resources.ResourceLoader loader;

        public LyricsView()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            loader = new Windows.ApplicationModel.Resources.ResourceLoader();
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
            statusTextBlock.Text = loader.GetString("Connecting") + "...";
            webView1.ContentLoading += webView1_ContentLoading;
            webView1.NavigationStarting += webView1_NavigationStarting;
            webView1.DOMContentLoaded += webView1_DOMContentLoaded;
            string[] array = ParamConvert.ToStringArray((string)e.NavigationParameter);
            artist = array[0];
            title = array[1];
            songId = Int32.Parse(array[2]);
            original = true;
            appBarSave.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            LoadLyrics();
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

        private void LoadLyrics()
        {
            lyrics = DatabaseManager.GetLyrics(songId);
            if (string.IsNullOrEmpty(lyrics))
            {
                WebVisibility(true);
                ShowLyrics();
            }
            else
            {
                original = false;
                WebVisibility(false);
                titleTB.Text = title;
                artistTB.Text = artist;
                lyricsTB.Text = lyrics;
            }
        }

        async private void ShowLyrics()
        {
            statusTextBlock.Text = loader.GetString("Connecting") + "...";
            statusTextBlock.Visibility = Visibility.Visible;
            webView1.Visibility = Visibility.Collapsed;
            string result = await ReadDataFromWeb("http://lyrics.wikia.com/api.php?action=lyrics&artist=" + artist + "&song=" + title + "&fmt=realjson");
            if (result == null || result == "")
            {
                statusTextBlock.Text = loader.GetString("ConnectionError");
                statusTextBlock.Visibility = Visibility.Visible;
                return;
            }
            JsonValue jsonList;
            bool isJson = JsonValue.TryParse(result,out jsonList);
            if (isJson)
            {
                address = jsonList.GetObject().GetNamedString("url");
                address += "?useskin=wikiamobile";
                try
                {
                    Uri a = new Uri(address);
                    webView1.Navigate(a);
                    webView1.Visibility = Visibility.Visible;
                    statusTextBlock.Visibility = Visibility.Collapsed;
                }
                catch (FormatException e)
                {
                    statusTextBlock.Text = loader.GetString("ConnectionError");
                    statusTextBlock.Visibility = Visibility.Visible;
                }
            }
            else
            {
                statusTextBlock.Text = loader.GetString("ConnectionError");
                statusTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }           
        }

        async private Task<string> ReadDataFromWeb(string a)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(new Uri(a));
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        private bool IsAllowedUri(Uri uri)
        {
            return uri.ToString().Contains("lyrics.wikia.com");
        }

        private void webView1_NavigationStarting(object sender, WebViewNavigationStartingEventArgs args)
        {
            if (!IsAllowedUri(args.Uri))
                args.Cancel = true;
        }

        private void webView1_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            if (args.Uri != null)
            {
                statusTextBlock.Text = "Loading content for " + args.Uri.ToString();
            }
        }

        void webView1_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            if (original)
            {
                //ParseLyrics();
                original = false;
            }
        }

        private async void ParseLyrics()
        {
            try
            {
                string html = await webView1.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });
                int i0 = html.IndexOf("Tap what's hiding behind");
                if (i0 < 0)
                {
                    int i1 = html.IndexOf("div class=\"lyricbox");
                    if (i1 > 0)
                    {
                        int i2 = html.IndexOf("</script>", i1) + "</script>".Length;
                        int i3 = html.IndexOf("<script>", i2);

                        string text = html.Substring(i2, i3 - i2);
                        lyrics = text.Replace("<br>", "\n").Replace("<b>", "").Replace("</b>", "").Replace("&amp;", "&").Replace("<i>","(").Replace("</i>", ")");
                        artistTB.Text = artist;
                        titleTB.Text = title;
                        lyricsTB.Text = lyrics;

                        SaveLyrics();
                    }
                }
            }
            catch (Exception ex)
            {
                NextPlayerDataLayer.Diagnostics.Logger.Save("Lyrics ParseLyrics() " + "\n" + address + " " + artist + " " + title + "\n" + ex.Message);
                NextPlayerDataLayer.Diagnostics.Logger.SaveToFile();
            }
        }

        private async void SaveLyrics()
        {
            await DatabaseManager.UpdateLyricsAsync(songId, lyrics);
            SaveLater.Current.SaveLyricsLater(songId, lyrics);
        }
        //private void webView1_FrameNavigationCompleted(WebView sender, WebViewContentLoadingEventArgs args)
        //{
        //    if (args.Uri != null)
        //    {
        //        statusTextBlock.Text = "Cannot connect. " + args.Uri.ToString();
        //    }
        //}

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            artist = editArtist.Text;
            title = editTitle.Text;
            FlyoutBase.GetAttachedFlyout(this).Hide();
            WebVisibility(true);
            //appBarSave.Visibility = Windows.UI.Xaml.Visibility.Visible;
            ShowLyrics();
        }

        private void edit_Click(object sender, RoutedEventArgs e)
        {
            editArtist.Text = artist;
            editTitle.Text = title;
            FlyoutBase.SetAttachedFlyout(this, (FlyoutBase)this.Resources["EditFlyout"]);
            FlyoutBase.ShowAttachedFlyout(this);
        }

        private void WebVisibility(bool visible)
        {
            if (visible)
            {
                WebGrid.Visibility = Visibility.Visible;
                LyricsGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                WebGrid.Visibility = Visibility.Collapsed;
                LyricsGrid.Visibility = Visibility.Visible;
            }
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            //ParseLyrics();
        }

    }
}
