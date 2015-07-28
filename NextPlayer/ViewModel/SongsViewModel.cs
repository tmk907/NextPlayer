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
using NextPlayerDataLayer.Helpers;
using NextPlayer.Converters;

namespace NextPlayer.ViewModel
{
    public class SongsViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private int index;
        private string genre;
        private bool loaded;

        public SongsViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            PageTitle = loader.GetString("SongsPageTitle");
            index = 0;
            genre = null;
        }

        public string PageTitle
        {
            get;
            private set;
        }

        /// <summary>
        /// The <see cref="Songs" /> property's name.
        /// </summary>
        public const string SongsPropertyName = "Songs";

        private ObservableCollection<GroupedOC<SongItem>> songs = new ObservableCollection<GroupedOC<SongItem>>();

        /// <summary>
        /// Sets and gets the Songs property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<GroupedOC<SongItem>> Songs
        {
            get
            {
                if (songs.Count == 0)
                {
                    if (IsInDesignMode)
                    {
                        ObservableCollection<SongItem> a = new ObservableCollection<SongItem>();

                        for (int i = 0; i < 5; i++)
                        {
                            a.Add(new SongItem());
                        }
                    }
                    else
                    {
                        LoadSongs();
                    }
                }
                return songs;
            }

            set
            {
                if (songs == value)
                {
                    return;
                }

                songs = value;
                RaisePropertyChanged(SongsPropertyName);
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

        private RelayCommand<SongItem> itemClicked;

        /// <summary>
        /// Gets the ItemClicked.
        /// </summary>
        public RelayCommand<SongItem> ItemClicked
        {
            get
            {
                return itemClicked
                    ?? (itemClicked = new RelayCommand<SongItem>(
                    item =>
                    {
                        bool find = false;
                        int i = 0;
                        List<SongItem> list = new List<SongItem>();
                        foreach (var a in Songs)
                        {
                            foreach (var b in a)
                            {
                                list.Add(b);
                                if (b.SongId == item.SongId)
                                {
                                    find = true;
                                    index = i;
                                }
                                i++;
                            }
                        }
                        if (!find) index = 0;

                        ApplicationSettingsHelper.SaveSongIndex(index);
                        DatabaseManager.InsertNewNowPlayingPlaylist(list);

                        navigationService.NavigateTo(ViewNames.NowPlayingView, item.SongId);
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
                        
                        //Songs = Grouped.CreateGrouped<SongItem>(DatabaseManager.GetSongItems(), x => x.Title);
                        //LoadSongs();
                        
                        //if (genre == null)
                        //{
                        //    
                        //}
                        //else
                        //{
                        //    Songs = Grouped.CreateGrouped <SongItem>(DatabaseManager.GetSongItemsFromGenre(genre),x => x.Title);
                        //}
                    }));
            }
        }

        private async Task LoadSongs()
        {
            var a = await DatabaseManager.GetSongItemsAsync();
            Songs = Grouped.CreateGrouped<SongItem>(a, x => x.Title);
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {
            index = 0;
            if (state != null)
            {
                if (state.ContainsKey("index"))
                {
                    index = (int) state["index"];
                }
            }
            genre = null;
            if (parameter!=null)
            {
                if (parameter.GetType() == typeof(string))
                {
                    String[] s = ParamConvert.ToStringArray(parameter as string);
                    if (s[0].Equals("genre")) genre = s[1];
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