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
using NextPlayerDataLayer.Helpers;
using NextPlayer.Converters;
using Windows.UI.Xaml.Controls;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using NextPlayerDataLayer.Constants;
using Windows.ApplicationModel.Resources;
using Windows.UI.StartScreen;

namespace NextPlayer.ViewModel
{
    public class FoldersViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        int index;

        public FoldersViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            MediaImport.MediaImported += new MediaImportedHandler(OnLibraryUpdated);
        }

        private void OnLibraryUpdated(string s)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                LoadFolders();
            });
        }

        /// <summary>
        /// The <see cref="Folders" /> property's name.
        /// </summary>
        public const string FoldersPropertyName = "Folders";

        private ObservableCollection<FolderItem> folders = new ObservableCollection<FolderItem>();

        /// <summary>
        /// Sets and gets the Folders property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<FolderItem> Folders
        {
            get
            {
                if (folders.Count == 0)
                {
                    if (IsInDesignMode)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            folders.Add(new FolderItem("folder aa fghfghfh dfg asdasd" + i.ToString(),"a", i));
                        }
                    }
                    else
                    {
                        LoadFolders();
                    }
                }
                return folders;
            }

            set
            {
                if (folders == value)
                {
                    return;
                }

                folders = value;
                RaisePropertyChanged(FoldersPropertyName);
            }
        }

        private RelayCommand<FolderItem> itemClicked;

        /// <summary>
        /// Gets the ItemClicked.
        /// </summary>
        public RelayCommand<FolderItem> ItemClicked
        {
            get
            {
                return itemClicked
                    ?? (itemClicked = new RelayCommand<FolderItem>(
                    item =>
                    {
                        index = Folders.IndexOf(item);
                        String[] s = new String[3];
                        s[0] = "folder";
                        s[1] = item.Folder;
                        s[2] = item.Directory;
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

        //private RelayCommand loadItems;

        ///// <summary>
        ///// Gets the LoadItems.
        ///// </summary>
        //public RelayCommand LoadItems
        //{
        //    get
        //    {
        //        return loadItems
        //            ?? (loadItems = new RelayCommand(
        //            () =>
        //            {
        //                Folders = DatabaseManager.GetFolderItems();
        //            }));
        //    }
        //}

        private RelayCommand<FolderItem> playNow;

        /// <summary>
        /// Gets the PlayNow.
        /// </summary>
        public RelayCommand<FolderItem> PlayNow
        {
            get
            {
                return playNow
                    ?? (playNow = new RelayCommand<FolderItem>(
                    item =>
                    {
                        index = Folders.IndexOf(item);
                        var g = DatabaseManager.GetSongItemsFromFolder(item.Directory);
                        Library.Current.SetNowPlayingList(g);
                        ApplicationSettingsHelper.SaveSongIndex(0);
                        navigationService.NavigateTo(ViewNames.NowPlayingView, "start");
                    }));
            }
        }

        private RelayCommand<FolderItem> addToNowPlaying;

        /// <summary>
        /// Gets the AddToNowPlaying.
        /// </summary>
        public RelayCommand<FolderItem> AddToNowPlaying
        {
            get
            {
                return addToNowPlaying
                    ?? (addToNowPlaying = new RelayCommand<FolderItem>(
                    item =>
                    {
                        AddToNowPlayingAsync(item);
                    }));
            }
        }

        private RelayCommand<FolderItem> addToPlaylist;

        /// <summary>
        /// Gets the AddToPlaylist.
        /// </summary>
        public RelayCommand<FolderItem> AddToPlaylist
        {
            get
            {
                return addToPlaylist
                    ?? (addToPlaylist = new RelayCommand<FolderItem>(
                    item =>
                    {
                        index = Folders.IndexOf(item);
                        String[] s = new String[3];
                        s[0] = "folder";
                        s[1] = item.Folder;
                        s[2] = item.Directory;
                        navigationService.NavigateTo(ViewNames.AddToPlaylistView, ParamConvert.ToString(s));
                    }));
            }
        }
        public async void AddToNowPlayingAsync(FolderItem item)
        {
            var g = await DatabaseManager.GetSongItemsFromFolderAsync(item.Directory);
            Library.Current.AddToNowPlaying(g);
        }


        private RelayCommand<FolderItem> pinFolder;

        /// <summary>
        /// Gets the PinFolder.
        /// </summary>
        public RelayCommand<FolderItem> PinFolder
        {
            get
            {
                return pinFolder
                    ?? (pinFolder = new RelayCommand<FolderItem>(
                    p =>
                    {
                        index = Folders.IndexOf(p);
                        Pin(p);
                    }));
            }
        }
        public async void Pin(FolderItem folder)
        {
            int id = ApplicationSettingsHelper.ReadTileIdValue() + 1;
            string tileId = AppConstants.TileId + id.ToString();
            ApplicationSettingsHelper.SaveTileIdValue(id);

            string displayName = "Next Player";
            string tileActivationArguments = ParamConvert.ToString(new string[] { "folder", folder.Directory });
            Uri square150x150Logo = new Uri("ms-appx:///Assets/AppImages/Logo/Logo.png");

            SecondaryTile secondaryTile = new SecondaryTile(tileId,
                                                displayName,
                                                tileActivationArguments,
                                                square150x150Logo,
                                                TileSize.Wide310x150);
            secondaryTile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/AppImages/WideLogo/WideLogo.png");
            secondaryTile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/AppImages/Square71x71Logo/Square71x71LogoTr.png");


            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileId, tileId);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileName, folder.Folder);
            ResourceLoader loader = new ResourceLoader();
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileType, loader.GetString("Folder"));

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

        private RelayCommand<FolderItem> share;

        /// <summary>
        /// Gets the Share.
        /// </summary>
        public RelayCommand<FolderItem> Share
        {
            get
            {
                return share
                    ?? (share = new RelayCommand<FolderItem>(
                    item =>
                    {
                        index = Folders.IndexOf(item);
                        String[] s = new String[3];
                        s[0] = "folder";
                        s[1] = item.Directory;
                        navigationService.NavigateTo(ViewNames.BluetoothShare, ParamConvert.ToString(s));
                    }));
            }
        }

        private async void LoadFolders()
        {
            Folders = await DatabaseManager.GetFolderItemsAsync();
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