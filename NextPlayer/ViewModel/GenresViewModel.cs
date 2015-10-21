using NextPlayer.Constants;
using NextPlayerDataLayer.Model;
using NextPlayerDataLayer.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextPlayer.Converters;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using NextPlayerDataLayer.Helpers;
using NextPlayerDataLayer.Constants;
using Windows.UI.StartScreen;
using Windows.ApplicationModel.Resources;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace NextPlayer.ViewModel
{
    public class GenresViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private int index;

        public GenresViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            MediaImport.MediaImported += new MediaImportedHandler(OnLibraryUpdated);
        }

        private void OnLibraryUpdated(string s)
        {
            LoadGenres();
        }

        /// <summary>
        /// The <see cref="Playlists" /> property's name.
        /// </summary>
        public const string GenresPropertyName = "Genres";

        private ObservableCollection<GenreItem> genres = new ObservableCollection<GenreItem>();

        /// <summary>
        /// Sets and gets the Playlists property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<GenreItem> Genres
        {
            get
            {
                if (genres.Count == 0)
                {
                    if (IsInDesignMode)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            genres.Add(new GenreItem(TimeSpan.Zero,"genreaaaaa fghfghfh dfg asdasd" + i.ToString(), i));
                        }
                    }
                    else
                    {
                        LoadGenres();
                    }
                }
                return genres;
            }

            set
            {
                if (genres == value)
                {
                    return;
                }

                genres = value;
                RaisePropertyChanged(GenresPropertyName);
            }
        }

        private RelayCommand<GenreItem> itemClicked;

        /// <summary>
        /// Gets the ItemClicked.
        /// </summary>
        public RelayCommand<GenreItem> ItemClicked
        {
            get
            {
                return itemClicked
                    ?? (itemClicked = new RelayCommand<GenreItem>(
                    item =>
                    {
                        index = Genres.IndexOf(item);
                        String[] s = new String[2];
                        s[0] = "genre";
                        s[1] = item.GenreParam;
                        navigationService.NavigateTo(ViewNames.PlaylistView, ParamConvert.ToString(s));
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

                        if (l.Items.Count > 0 && index > 0)
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
                        Genres = DatabaseManager.GetGenreItems();
                    }));
            }
        }

        private RelayCommand<GenreItem> playNow;

        /// <summary>
        /// Gets the PlayNow.
        /// </summary>
        public RelayCommand<GenreItem> PlayNow
        {
            get
            {
                return playNow
                    ?? (playNow = new RelayCommand<GenreItem>(
                    item =>
                    {
                        var g = DatabaseManager.GetSongItemsFromGenre(item.GenreParam);
                        Library.Current.SetNowPlayingList(g);
                        ApplicationSettingsHelper.SaveSongIndex(0);
                        navigationService.NavigateTo(ViewNames.NowPlayingView, "start");
                    }));
            }
        }

        private RelayCommand<GenreItem> addToNowPlaying;

        /// <summary>
        /// Gets the AddToNowPlaying.
        /// </summary>
        public RelayCommand<GenreItem> AddToNowPlaying
        {
            get
            {
                return addToNowPlaying
                    ?? (addToNowPlaying = new RelayCommand<GenreItem>(
                    item =>
                    {
                        AddToNowPlayingAsync(item);
                    }));
            }
        }

        private RelayCommand<GenreItem> addToPlaylist;

        /// <summary>
        /// Gets the AddToPlaylist.
        /// </summary>
        public RelayCommand<GenreItem> AddToPlaylist
        {
            get
            {
                return addToPlaylist
                    ?? (addToPlaylist = new RelayCommand<GenreItem>(
                    item =>
                    {
                        String[] s = new String[2];
                        s[0] = "genre";
                        s[1] = item.GenreParam;
                        navigationService.NavigateTo(ViewNames.AddToPlaylistView, ParamConvert.ToString(s));
                    }));
            }
        }
        public async void AddToNowPlayingAsync(GenreItem item)
        {
            var g = await DatabaseManager.GetSongItemsFromGenreAsync(item.GenreParam);
            Library.Current.AddToNowPlaying(g);
        }


        private RelayCommand<GenreItem> pinGenre;

        /// <summary>
        /// Gets the PinGenre.
        /// </summary>
        public RelayCommand<GenreItem> PinGenre
        {
            get
            {
                return pinGenre
                    ?? (pinGenre = new RelayCommand<GenreItem>(
                    p =>
                    {
                        Pin(p);
                    }));
            }
        }
        public async void Pin(GenreItem genre)
        {
            int id = ApplicationSettingsHelper.ReadTileIdValue() + 1;
            string tileId = AppConstants.TileId + id.ToString();
            ApplicationSettingsHelper.SaveTileIdValue(id);

            string displayName = "Next Player";
            string tileActivationArguments = ParamConvert.ToString(new string[] { "genre", genre.GenreParam });
            Uri square150x150Logo = new Uri("ms-appx:///Assets/AppImages/Logo/Logo.png");

            SecondaryTile secondaryTile = new SecondaryTile(tileId,
                                                displayName,
                                                tileActivationArguments,
                                                square150x150Logo,
                                                TileSize.Wide310x150);
            secondaryTile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/AppImages/WideLogo/WideLogo.png");
            secondaryTile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/AppImages/Square71x71Logo/Square71x71LogoTr.png");


            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileId, tileId);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileName, genre.Genre);
            ResourceLoader loader = new ResourceLoader();
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileType, loader.GetString("Genre"));

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

        private RelayCommand<GenreItem> share;

        /// <summary>
        /// Gets the Share.
        /// </summary>
        public RelayCommand<GenreItem> Share
        {
            get
            {
                return share
                    ?? (share = new RelayCommand<GenreItem>(
                    item =>
                    {
                        String[] s = new String[3];
                        s[0] = "genre";
                        s[1] = item.GenreParam;
                        navigationService.NavigateTo(ViewNames.BluetoothShare, ParamConvert.ToString(s));
                    }));
            }
        }

        private async void LoadGenres()
        {
            Genres = await DatabaseManager.GetGenreItemsAsync();
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

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }
    }
}