using NextPlayer.Common;
using NextPlayer.Converters;
using NextPlayerDataLayer.Model;
using NextPlayerDataLayer.Services;
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
    public sealed partial class AddToPlaylist : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private string genre;
        private string directory;
        private int songId;
        private string artist;
        private string album;
        private string nowPlaying;

        public AddToPlaylist()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            songId = -1;
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
            if (e.NavigationParameter != null)
            {
                List<PlaylistItem> list = DatabaseManager.SelectPlainPlaylists();
                DataContext = list;
                String[] s = ParamConvert.ToStringArray(e.NavigationParameter as string);
                if (s[0] == "genre")
                {
                    genre = s[1];
                }
                else if (s[0] == "folder")
                {
                    directory = s[2];
                }
                else if (s[0] == "song")
                {
                    songId = Int32.Parse(s[1]);
                }
                else if (s[0] == "artist")
                {
                    artist = s[1];
                }
                else if (s[0] == "album")
                {
                    album = s[1];
                    artist = s[3];
                }
                else if (s[0] == "nowPlaying")
                {
                    nowPlaying = s[0];
                }
            }
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

        private void item_Click(object sender, ItemClickEventArgs e)
        {
            PlaylistItem p = (PlaylistItem)(e.ClickedItem);
            if (genre != null)
            {
                DatabaseManager.AddGenreToPlaylistAsync(genre, p.Id);
            }
            else if (directory != null)
            {
                DatabaseManager.AddFolderToPlaylistAsync(directory,p.Id);
            }
            else if (artist != null)
            {
                DatabaseManager.AddArtistToPlaylistAsync(artist, p.Id);
            }
            else if (album != null)
            {
                DatabaseManager.AddAlbumToPlaylistAsync(album, artist, p.Id);
            }
            else if (songId > -1)
            {
                DatabaseManager.AddSongToPlaylist(songId, p.Id);
            }
            else if (nowPlaying != null)
            {
                DatabaseManager.AddNowPlayingToPlaylist(p.Id);
            }
            Frame.GoBack();
        }
    }
}
