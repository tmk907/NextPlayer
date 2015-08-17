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
using NextPlayerDataLayer.Helpers;
using NextPlayer.Converters;
using Windows.UI.Xaml.Controls;

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
            LoadFolders();
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