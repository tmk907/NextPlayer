using NextPlayerUniversal.Constants;
using NextPlayerUniversal.Model;
using NextPlayerUniversal.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextPlayerUniversal.Common;
using Windows.UI.Xaml.Controls;
using NextPlayerUniversal.Converters;
using NextPlayerUniversal.Helpers;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.ApplicationModel.Resources;
using Windows.UI.StartScreen;

namespace NextPlayerUniversal.ViewModel
{
    public class ArtistsViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private int index;

        public ArtistsViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            MediaImport.MediaImported += new MediaImportedHandler(OnLibraryUpdated);
        }

        private void OnLibraryUpdated(string s)
        {
            LoadArtists();
        }

        /// <summary>
        /// The <see cref="Artists" /> property's name.
        /// </summary>
        public const string ArtistsPropertyName = "Artists";

        private ObservableCollection<GroupedOC<ArtistItem>> artists = new ObservableCollection<GroupedOC<ArtistItem>>();

        /// <summary>
        /// Sets and gets the Artists property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<GroupedOC<ArtistItem>> Artists
        {
            get
            {
                if (artists.Count == 0)
                {
                    if (IsInDesignMode)
                    {
                        ObservableCollection<ArtistItem> a = new ObservableCollection<ArtistItem>();

                        for (int i = 0; i < 5; i++)
                        {
                            a.Add(new ArtistItem());
                        }
                        artists = Grouped.CreateGrouped<ArtistItem>(a, x => x.Artist);
                    }
                    else
                    {
                        LoadArtists();
                    }
                }
                
                return artists;
            }

            set
            {
                if (artists == value)
                {
                    return;
                }

                artists = value;
                RaisePropertyChanged(ArtistsPropertyName);
            }
        }

        private RelayCommand<ArtistItem> playNow;

        /// <summary>
        /// Gets the PlayNow.
        /// </summary>
        public RelayCommand<ArtistItem> PlayNow
        {
            get
            {
                return playNow
                    ?? (playNow = new RelayCommand<ArtistItem>(
                    item =>
                    {
                        var g = DatabaseManager.GetSongItemsFromArtist(item.Artist);
                        g.OrderBy(s => s.Album).ThenBy(t=>t.TrackNumber);
                        Library.Current.SetNowPlayingList(g);
                        ApplicationSettingsHelper.SaveSongIndex(0);
                        navigationService.NavigateTo(ViewNames.NowPlayingView, "start");
                    }));
            }
        }

        private RelayCommand<ArtistItem> addToNowPlaying;

        /// <summary>
        /// Gets the AddToNowPlaying.
        /// </summary>
        public RelayCommand<ArtistItem> AddToNowPlaying
        {
            get
            {
                return addToNowPlaying
                    ?? (addToNowPlaying = new RelayCommand<ArtistItem>(
                    item =>
                    {
                        AddToNowPlayingAsync(item);
                    }));
            }
        }
        public async void AddToNowPlayingAsync(ArtistItem item)
        {
            var g = await DatabaseManager.GetSongItemsFromArtistAsync(item.Artist);
            g.OrderBy(s => s.Album).ThenBy(t => t.TrackNumber);
            Library.Current.AddToNowPlaying(g);
        }
        private RelayCommand<ArtistItem> addToPlaylist;

        /// <summary>
        /// Gets the AddToPlaylist.
        /// </summary>
        public RelayCommand<ArtistItem> AddToPlaylist
        {
            get
            {
                return addToPlaylist
                    ?? (addToPlaylist = new RelayCommand<ArtistItem>(
                    item =>
                    {
                        String[] s = new String[2];
                        s[0] = "artist";
                        s[1] = item.Artist;
                        navigationService.NavigateTo(ViewNames.AddToPlaylistView, ParamConvert.ToString(s));
                    }));
            }
        }

        private RelayCommand<ArtistItem> pinArtist;

        /// <summary>
        /// Gets the PinArtist.
        /// </summary>
        public RelayCommand<ArtistItem> PinArtist
        {
            get
            {
                return pinArtist
                    ?? (pinArtist = new RelayCommand<ArtistItem>(
                    p =>
                    {
                        Pin(p);
                    }));
            }
        }
        public async void Pin(ArtistItem artist)
        {
            int id = ApplicationSettingsHelper.ReadTileIdValue() + 1;
            string tileId = AppConstants.TileId + id.ToString();
            ApplicationSettingsHelper.SaveTileIdValue(id);

            string displayName = "Next Player";
            string tileActivationArguments = ParamConvert.ToString(new string[] { "artist", artist.Artist});
            Uri square150x150Logo = new Uri("ms-appx:///Assets/AppImages/Logo/Logo.png");

            SecondaryTile secondaryTile = new SecondaryTile(tileId,
                                                displayName,
                                                tileActivationArguments,
                                                square150x150Logo,
                                                TileSize.Wide310x150);
            secondaryTile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/AppImages/WideLogo/WideLogo.png");
#if WINDOWS_PHONE_APP
            secondaryTile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/AppImages/Square71x71Logo/Square71x71LogoTr.png");
#endif

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileId, tileId);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileName, artist.Artist);
            ResourceLoader loader = new ResourceLoader();
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileType, loader.GetString("Artist"));

            App.OnNewTilePinned = UpdateNewSecondaryTile;

            await secondaryTile.RequestCreateAsync();
        }

        public void UpdateNewSecondaryTile()
        {
            string name = ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.TileName) as string;
            string id = ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.TileId) as string;
            string type = ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.TileType) as string;

            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text02);
            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = type;
            tileTextAttributes[1].InnerText = name;

            XmlDocument wideTile = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Text09);
            XmlNodeList textAttr = wideTile.GetElementsByTagName("text");
            textAttr[0].InnerText = type;
            textAttr[1].InnerText = name;

            IXmlNode node = tileXml.ImportNode(wideTile.GetElementsByTagName("binding").Item(0), true);
            tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node);

            TileNotification tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForSecondaryTile(id).Update(tileNotification);
        }

        private RelayCommand<ArtistItem> itemClicked;

        /// <summary>
        /// Gets the ItemClicked.
        /// </summary>
        public RelayCommand<ArtistItem> ItemClicked
        {
            get
            {
                return itemClicked
                    ?? (itemClicked = new RelayCommand<ArtistItem>(
                    item =>
                    {
                        bool find = false;
                        int i = 0;
                        foreach (var a in Artists)
                        {
                            foreach (var b in a)
                            {
                                if (b.Artist == item.Artist)
                                {
                                    find = true;
                                    index = i;
                                    break;
                                }
                                i++;
                            }
                            if (find) break;
                        }
                        if (!find) index = 0;
                        String[] s = new String[2];
                        s[0] = "artist";
                        s[1] = item.Artist;
                        navigationService.NavigateTo(ViewNames.AlbumsView, ParamConvert.ToString(s));
                    }));
            }
        }

        private RelayCommand loadItems;

        /// <summary>
        /// Gets the LoadItems.
        /// </summary>
        public RelayCommand LoadItems
        {
            get
            {
                return loadItems
                    ?? (loadItems = new RelayCommand(
                    () =>
                    {
                        Artists = Grouped.CreateGrouped<ArtistItem>(DatabaseManager.GetArtistItems(), x => x.Artist);
                    }));
            }
        }


        private RelayCommand<object> scrollListView;

        /// <summary>
        /// Gets the ScrollListView.
        /// </summary>
        public RelayCommand<object> ScrollListView
        {
            get
            {
                return scrollListView
                    ?? (scrollListView = new RelayCommand<object>(
                    p =>
                    {
                        ListView l = (ListView)p;
                        if (l.Items.Count > 0)
                        {
                            SemanticZoomLocation loc = new SemanticZoomLocation();
                            l.SelectedIndex = index;
                            loc.Item = l.SelectedItem;
                            l.UpdateLayout();
                            l.MakeVisible(loc);
                            l.ScrollIntoView(l.SelectedItem, ScrollIntoViewAlignment.Leading);
                        }
                    }));
            }
        }

        private async void LoadArtists()
        {
            var a = await DatabaseManager.GetArtistItemsAsync();
            Artists = Grouped.CreateGrouped<ArtistItem>(a, x => x.Artist);
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {
            index = 0;
            if (state != null)
            {
                if (state.ContainsKey("index"))
                {
                    index = (int)state["index"];
                }
            }
        }

        public void Deactivate(Dictionary<string, object> state)
        {
            state["index"] = index;
        }

        
    }
}