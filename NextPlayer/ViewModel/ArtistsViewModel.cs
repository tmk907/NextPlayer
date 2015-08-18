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
using NextPlayerDataLayer.Common;
using Windows.UI.Xaml.Controls;
using NextPlayer.Converters;
using NextPlayerDataLayer.Helpers;

namespace NextPlayer.ViewModel
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

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }
    }
}