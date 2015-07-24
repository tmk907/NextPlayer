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

namespace NextPlayer.ViewModel
{
    public class SongsViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private int index;

        public SongsViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            PageTitle = "songs";
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
                        songs = Grouped.CreateGrouped<SongItem>(a, x => x.Title);
                    }
                    //songs = Grouped.CreateGrouped<SongItem>(DatabaseManager.GetSongItems(), x => x.Title);
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
                        //index
                        //Library.Current.SetNowPlayingList(Library.Current.Songs);
                        //Library.Current.SaveCurrentSongIndex(((SongItem)e.ClickedItem).SongId);
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
                    }));
            }
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {
            if (state != null)
            {
                if (state.ContainsKey("index"))
                {
                    index = (int) state["index"];
                }
            }
            if (parameter == null)
            {
                Songs = Grouped.CreateGrouped<SongItem>(DatabaseManager.GetSongItems(), x => x.Title);
            }
            else
            {
                Songs = (ObservableCollection<GroupedOC<SongItem>>) Library.Current.GetSongsGrouped((string) parameter);
            }
        }

        public void Deactivate(Dictionary<string, object> state)
        {
            state.Add("index", index);
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }
    }
}