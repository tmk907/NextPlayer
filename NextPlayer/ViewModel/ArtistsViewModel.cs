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

namespace NextPlayer.ViewModel
{
    public class ArtistsViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private int index;

        public ArtistsViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
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