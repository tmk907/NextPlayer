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
                        SemanticZoomLocation loc = new SemanticZoomLocation();
                        l.SelectedIndex = index;
                        loc.Item = l.SelectedItem;
                        l.UpdateLayout();
                        l.MakeVisible(loc);
                        l.ScrollIntoView(l.SelectedItem, ScrollIntoViewAlignment.Leading);
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
                        navigationService.NavigateTo(ViewNames.AlbumView, s);
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
                        if (artist == null)
                        {
                            Albums = Grouped.CreateGrouped<AlbumItem>(DatabaseManager.GetAlbumItems(), x => x.Album);
                        }
                        else
                        {
                            Albums = Grouped.CreateGrouped<AlbumItem>(DatabaseManager.GetAlbumItems(artist), x => x.Album);
                        }
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
            artist = null;
            if (parameter != null)
            {
                if (parameter.GetType() == typeof(String[]))
                {
                    if (((String[])parameter)[0].Equals("artist")) artist = ((String[])parameter)[1];
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