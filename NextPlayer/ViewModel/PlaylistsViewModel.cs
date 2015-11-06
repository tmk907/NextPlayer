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
using NextPlayerDataLayer.Helpers;
using Windows.UI.StartScreen;
using NextPlayerDataLayer.Constants;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.ApplicationModel.Resources;


namespace NextPlayer.ViewModel
{
    public class PlaylistsViewModel :ViewModelBase, INavigable
    {
        private INavigationService navigationService;

        public PlaylistsViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        /// <summary>
        /// The <see cref="Playlists" /> property's name.
        /// </summary>
        public const string PlaylistsPropertyName = "Playlists";

        private ObservableCollection<PlaylistItem> playlists = new ObservableCollection<PlaylistItem>();

        /// <summary>
        /// Sets and gets the Playlists property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<PlaylistItem> Playlists
        {
            get
            {
                if (playlists.Count == 0)
                {
                    if (IsInDesignMode)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            playlists.Add(new PlaylistItem(i, false, i.ToString()));
                        }
                    }
                    else playlists = DatabaseManager.GetPlaylistItems();
                }
                return playlists;
            }

            set
            {
                if (playlists == value)
                {
                    return;
                }

                playlists = value;
                RaisePropertyChanged(PlaylistsPropertyName);
            }
        }

        private RelayCommand<PlaylistItem> itemClicked;

        /// <summary>
        /// Gets the ItemClicked.
        /// </summary>
        public RelayCommand<PlaylistItem> ItemClicked
        {
            get
            {
                return itemClicked
                    ?? (itemClicked = new RelayCommand<PlaylistItem>(
                    item =>
                    {
                        String[] s = new String[3];
                        s[0] = item.IsSmart?"smart":"plain";
                        s[1] = item.Id.ToString();
                        s[2] = item.Name;
                        navigationService.NavigateTo(ViewNames.PlaylistView,ParamConvert.ToString(s));
                    }));
            }
        }

        private RelayCommand smartPlaylistClick;

        /// <summary>
        /// Gets the SmartPlaylistClick.
        /// </summary>
        public RelayCommand SmartPlaylistClick
        {
            get
            {
                return smartPlaylistClick
                    ?? (smartPlaylistClick = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo(ViewNames.NewSmartPlaylistView);
                    }));
            }
        }

        private RelayCommand<PlaylistItem> editSmartPlaylist;

        /// <summary>
        /// Gets the EditSmartPlaylist.
        /// </summary>
        public RelayCommand<PlaylistItem> EditSmartPlaylist
        {
            get
            {
                return editSmartPlaylist
                    ?? (editSmartPlaylist = new RelayCommand<PlaylistItem>(
                    item =>
                    {
                        navigationService.NavigateTo(ViewNames.NewSmartPlaylistView,item.Id);
                    }));
            }
        }

        public void PlayNow(PlaylistItem playlist)
        {
            ObservableCollection<SongItem> songList = new ObservableCollection<SongItem>();
            if (playlist.IsSmart)
            {
                songList = DatabaseManager.GetSongItemsFromSmartPlaylist(playlist.Id);
            }
            else
            {
                songList = DatabaseManager.GetSongItemsFromPlainPlaylist(playlist.Id);
            }
                        
            Library.Current.SetNowPlayingList(songList);
            ApplicationSettingsHelper.SaveSongIndex(0);
            navigationService.NavigateTo(ViewNames.NowPlayingView, "start");
        }

        public void AddToNowPlaying(PlaylistItem playlist)
        {
            if (playlist.IsSmart)
            {
                Library.Current.AddToNowPlaying(DatabaseManager.GetSongItemsFromSmartPlaylist(playlist.Id));
            }
            else
            {
                Library.Current.AddToNowPlaying(DatabaseManager.GetSongItemsFromPlainPlaylist(playlist.Id));
            }
        }
                       
        public void AddPlainPlaylist(string name)
        {
            int id = DatabaseManager.InsertPlainPlaylist(name);
            Playlists.Add(new PlaylistItem(id,false,name));
        }

        public void DeletePlaylist(PlaylistItem p)
        {
            if (p.IsSmart)
            {
                if (ApplicationSettingsHelper.PredefinedSmartPlaylistsId().ContainsKey(p.Id))
                {

                }
                else
                {
                    Playlists.Remove(p);
                    DatabaseManager.DeleteSmartPlaylist(p.Id);
                }
            }
            else
            {
                Playlists.Remove(p);
                DatabaseManager.DeletePlainPlaylist(p.Id);
            }
        }

        public async void PinPlaylist(PlaylistItem p)
        {
            //string tileId = p.IsSmart ? AppConstants.TileId + p.Id + "smart": AppConstants.TileId + p.Id + "plain";
            int id = ApplicationSettingsHelper.ReadTileIdValue() + 1;
            string tileId = AppConstants.TileId + id.ToString();
            ApplicationSettingsHelper.SaveTileIdValue(id);

            if (!SecondaryTile.Exists(tileId))
            {
                string displayName = "Next Player";
                string tileActivationArguments = ParamConvert.ToString(new string[]{"playlist",p.Id.ToString(),p.IsSmart.ToString()});
                Uri square150x150Logo = new Uri("ms-appx:///Assets/AppImages/Logo/Logo.png");
                
                SecondaryTile secondaryTile = new SecondaryTile(tileId,
                                                    displayName,
                                                    tileActivationArguments,
                                                    square150x150Logo,
                                                    TileSize.Wide310x150);
                secondaryTile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/AppImages/WideLogo/WideLogo.png");
                secondaryTile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/AppImages/Square71x71Logo/Square71x71LogoTr.png");


                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileId, tileId);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileName, p.Name);
                ResourceLoader loader = new ResourceLoader();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileType, loader.GetString("Playlist"));
                
                App.OnNewTilePinned = UpdateNewSecondaryTile;

                await secondaryTile.RequestCreateAsync();
            }
        }

        public void UpdateNewSecondaryTile()
        {
            string name = ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.TileName) as string ;
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

        public void Share(PlaylistItem item)
        {
            String[] s = new String[3];
            s[0] = "playlist";
            if (item.IsSmart)
            {
                s[1] = "smart";
            }
            else
            {
                s[1] = "plain";
            }
            s[2] = item.Id.ToString();
            navigationService.NavigateTo(ViewNames.BluetoothShare, ParamConvert.ToString(s));
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {
            playlists.Clear();
        }

        public void Deactivate(Dictionary<string, object> state)
        {
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }
    }
}
