using NextPlayer.Constants;
using NextPlayerDataLayer.Model;
using NextPlayerDataLayer.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using NextPlayerDataLayer.Common;
using NextPlayer.Converters;
using NextPlayerDataLayer.Helpers;
using Windows.UI.StartScreen;
using NextPlayerDataLayer.Constants;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.Storage;

namespace NextPlayer.ViewModel
{
    public class AlbumsViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private int index;
        private string artistParam;
        

        public AlbumsViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            index = 0;
            MediaImport.MediaImported += new MediaImportedHandler(OnLibraryUpdated);
            App.SongUpdated += new SongUpdatedHandler(OnSongUpdated);
        }

        private void OnLibraryUpdated(string s)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                Albums.Clear();
                LoadAlbums();
            });
        }

        private void OnSongUpdated(int id)
        {
            Albums.Clear();
            LoadAlbums();
        }

        private RelayCommand<AlbumItem> playNow;

        /// <summary>
        /// Gets the PlayNow.
        /// </summary>
        public RelayCommand<AlbumItem> PlayNow
        {
            get
            {
                return playNow
                    ?? (playNow = new RelayCommand<AlbumItem>(
                    item =>
                    {
                        FindIndex(item);
                        var g = DatabaseManager.GetSongItemsFromAlbum(item.AlbumParam, item.ArtistParam);
                        Library.Current.SetNowPlayingList(g);
                        ApplicationSettingsHelper.SaveSongIndex(0);
                        navigationService.NavigateTo(ViewNames.NowPlayingView,"start");
                    }));
            }
        }

        private RelayCommand<AlbumItem> addToNowPlaying;

        /// <summary>
        /// Gets the AddToNowPlaying.
        /// </summary>
        public RelayCommand<AlbumItem> AddToNowPlaying
        {
            get
            {
                return addToNowPlaying
                    ?? (addToNowPlaying = new RelayCommand<AlbumItem>(
                    item =>
                    {
                        AddToNowPlayingAsync(item);
                    }));
            }
        }
        public async void AddToNowPlayingAsync(AlbumItem item)
        {
            var g = DatabaseManager.GetSongItemsFromAlbum(item.AlbumParam,item.ArtistParam);
            Library.Current.AddToNowPlaying(g);
        }
        private RelayCommand<AlbumItem> addToPlaylist;

        /// <summary>
        /// Gets the AddToPlaylist.
        /// </summary>
        public RelayCommand<AlbumItem> AddToPlaylist
        {
            get
            {
                return addToPlaylist
                    ?? (addToPlaylist = new RelayCommand<AlbumItem>(
                    item =>
                    {
                        FindIndex(item);
                        String[] s = new String[4];
                        s[0] = "album";
                        s[1] = item.AlbumParam;
                        s[2] = "artist";
                        s[3] = item.ArtistParam;
                        navigationService.NavigateTo(ViewNames.AddToPlaylistView, ParamConvert.ToString(s));
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

        private ObservableCollection<GroupedOC<AlbumItem>> allAlbums = new ObservableCollection<GroupedOC<AlbumItem>>();

        /// <summary>
        /// The <see cref="Albums" /> property's name.
        /// </summary>
        public const string AlbumsPropertyName = "Albums";

        private ObservableCollection<GroupedOC<AlbumItem>> albums = new ObservableCollection<GroupedOC<AlbumItem>>();

        /// <summary>
        /// Sets and gets the Albums property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<GroupedOC<AlbumItem>> Albums
        {
            get
            {
                if (IsInDesignMode)
                {
                    ObservableCollection<AlbumItem> a = new ObservableCollection<AlbumItem>();

                    for (int i = 0; i < 5; i++)
                    {
                        a.Add(new AlbumItem());
                    }
                    albums = Grouped.CreateGrouped<AlbumItem>(a, x => x.Album);
                }
                return albums;
            }

            set
            {
                if (albums == value)
                {
                    return;
                }

                albums = value;
                RaisePropertyChanged(AlbumsPropertyName);
            }
        }

        private RelayCommand<AlbumItem> itemClicked;

        /// <summary>
        /// Gets the ItemClicked.
        /// </summary>
        public RelayCommand<AlbumItem> ItemClicked
        {
            get
            {
                return itemClicked
                    ?? (itemClicked = new RelayCommand<AlbumItem>(
                    item =>
                    {
                        FindIndex(item);
                        String[] s = new String[4];
                        s[0] = "album";
                        s[1] = item.AlbumParam;
                        s[2] = "artist";
                        s[3] = item.ArtistParam;
                        navigationService.NavigateTo(ViewNames.AlbumView, ParamConvert.ToString(s));
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
                        LoadAlbums();
                    }));
            }
        }

        private void FindIndex(AlbumItem item)
        {
            bool find = false;
            int i = 0;
            foreach (var a in Albums)
            {
                foreach (var b in a)
                {
                    if (b.Album == item.Album)
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
        }

        private void LoadAlbums()
        {
            if (artistParam == null)
            {
                if (allAlbums.Count == 0)
                {
                    allAlbums = Grouped.CreateGrouped<AlbumItem>(DatabaseManager.GetAlbumItems(), x => x.Album);
                }
                Albums = allAlbums;
            }
            else
            {
                Albums = Grouped.CreateGrouped<AlbumItem>(DatabaseManager.GetAlbumItems(artistParam), x => x.Album);
            }
        }

        private RelayCommand<AlbumItem> pinAlbum;

        /// <summary>
        /// Gets the PinAlbum.
        /// </summary>
        public RelayCommand<AlbumItem> PinAlbum
        {
            get
            {
                return pinAlbum
                    ?? (pinAlbum = new RelayCommand<AlbumItem>(
                    p =>
                    {
                        FindIndex(p);
                        Pin(p);
                    }));
            }
        }

        public async void Pin(AlbumItem p)
        {
            int id = ApplicationSettingsHelper.ReadTileIdValue() + 1;
            string tileId = AppConstants.TileId + id.ToString();
            ApplicationSettingsHelper.SaveTileIdValue(id);
            
            if (!SecondaryTile.Exists(tileId))
            {
                string imageName = await Library.Current.SaveAlbumCover(p.AlbumParam, p.ArtistParam, tileId);
                string displayName = "Next Player";
                string tileActivationArguments = ParamConvert.ToString(new string[] { "album", p.AlbumParam, p.ArtistParam });
                Uri square150x150Logo = new Uri("ms-appx:///Assets/AppImages/Logo/Logo.png");

                SecondaryTile secondaryTile = new SecondaryTile(tileId,
                                                    displayName,
                                                    tileActivationArguments,
                                                    square150x150Logo,
                                                    TileSize.Square150x150);
                secondaryTile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/AppImages/WideLogo/WideLogo.png");
                secondaryTile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/AppImages/Square71x71Logo/Square71x71LogoTr.png");

                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileId, tileId);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileName, ParamConvert.ToString(new string[]{p.Album,p.AlbumArtist}));
                ResourceLoader loader = new ResourceLoader();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileType, loader.GetString("Album"));
                if (imageName.Contains(tileId))
                {
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileImage, "yes");
                }
                else
                {
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileImage, "no");
                }

                App.OnNewTilePinned = UpdateNewSecondaryTile;

                await secondaryTile.RequestCreateAsync();
            }
        }

        public async void UpdateNewSecondaryTile()
        {
            string name = ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.TileName) as string;
            string[] s = ParamConvert.ToStringArray(name);
            string id = ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.TileId) as string;
            string type = ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.TileType) as string;
            string hasImage = ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.TileImage) as string;

            XmlDocument tileXml;
            //XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text02);
            //string imagePath = "ms-appx:///Assets/AppImages/Logo/Logo.png";
            
            if (hasImage=="no")
            {
                tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text02);
            }
            else
            {
                tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150PeekImageAndText02);
                var tileImageAttributes = tileXml.GetElementsByTagName("image");
                tileImageAttributes[0].Attributes.GetNamedItem("src").NodeValue = "ms-appdata:///local/" + id + ".jpg";
                //tileImageAttributes[0].Attributes.GetNamedItem("alt").NodeValue = "album cover";
            }

            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = type;
            tileTextAttributes[1].InnerText = s[0] + "\n" + s[1];


            XmlDocument wideTile = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Text09);
            XmlNodeList textAttr = wideTile.GetElementsByTagName("text");
            textAttr[0].InnerText = type;
            textAttr[1].InnerText = s[0] + "\n" + s[1];

            IXmlNode node = tileXml.ImportNode(wideTile.GetElementsByTagName("binding").Item(0), true);
            tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node);

            TileNotification tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForSecondaryTile(id).Update(tileNotification);
        }

        private RelayCommand<AlbumItem> share;

        /// <summary>
        /// Gets the Share.
        /// </summary>
        public RelayCommand<AlbumItem> Share
        {
            get
            {
                return share
                    ?? (share = new RelayCommand<AlbumItem>(
                    item =>
                    {
                        FindIndex(item);
                        String[] s = new String[3];
                        s[0] = "album";
                        s[1] = item.AlbumParam;
                        s[2] = item.ArtistParam;
                        navigationService.NavigateTo(ViewNames.BluetoothShare, ParamConvert.ToString(s));
                    }));
            }
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
            artistParam = null;
            if (parameter != null)
            {
                if (parameter.GetType() == typeof(string))
                {
                    String[] s = ParamConvert.ToStringArray(parameter as string);
                    if (s[0].Equals("artist")) artistParam = s[1];
                }
            }
        }

        public void Deactivate(Dictionary<string, object> state)
        {
            state["index"] = index;
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }
    }
}