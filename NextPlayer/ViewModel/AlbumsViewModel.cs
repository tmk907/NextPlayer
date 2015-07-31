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
using Windows.UI.Xaml.Controls;
using NextPlayerDataLayer.Common;
using NextPlayer.Converters;
using NextPlayerDataLayer.Helpers;

namespace NextPlayer.ViewModel
{
    public class AlbumsViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private int index;
        private string artist;
        

        public AlbumsViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            index = 0;
            artist = null;
            MediaImport.MediaImported += new MediaImportedHandler(OnLibraryUpdated);
        }

        private void OnLibraryUpdated(string s)
        {
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
                        var g = DatabaseManager.GetSongItemsFromAlbum(item.Album, item.Artist);
                        Library.Current.SetNowPlayingList(g);
                        ApplicationSettingsHelper.SaveSongIndex(0);
                        navigationService.NavigateTo(ViewNames.NowPlayingView);
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
            var g = DatabaseManager.GetSongItemsFromAlbum(item.Album,item.Artist);
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
                        String[] s = new String[4];
                        s[0] = "album";
                        s[1] = item.Album;
                        s[2] = "artist";
                        s[3] = item.Artist;
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
                if (albums.Count == 0)
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
                    else
                    {

                    }
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
                        String[] s = new String[4];
                        s[0] = "album";
                        s[1] = item.Album;
                        s[2] = "artist";
                        s[3] = artist;
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

        private void LoadAlbums()
        {
            if (artist == null)
            {
                Albums = Grouped.CreateGrouped<AlbumItem>(DatabaseManager.GetAlbumItems(), x => x.Album);
            }
            else
            {
                Albums = Grouped.CreateGrouped<AlbumItem>(DatabaseManager.GetAlbumItems(artist), x => x.Album);
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
            artist = null;
            if (parameter != null)
            {
                if (parameter.GetType() == typeof(string))
                {
                    String[] s = ParamConvert.ToStringArray(parameter as string);
                    if (s[0].Equals("artist")) artist = s[1];
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