using NextPlayerUniversal.Constants;
using NextPlayerUniversal.Model;
using NextPlayerUniversal.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextPlayerUniversal.Helpers;

namespace NextPlayerUniversal.ViewModel
{
    public class SearchViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;

        public SearchViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        /// <summary>
        /// The <see cref="SearchResults" /> property's name.
        /// </summary>
        public const string SearchResultsPropertyName = "SearchResults";

        private ObservableCollection<SongItem> searchResults = new ObservableCollection<SongItem>();

        /// <summary>
        /// Sets and gets the SearchResults property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<SongItem> SearchResults
        {
            get
            {
                return searchResults;
            }

            set
            {
                if (searchResults == value)
                {
                    return;
                }

                searchResults = value;
                RaisePropertyChanged(SearchResultsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SearchQuery" /> property's name.
        /// </summary>
        public const string SearchQueryPropertyName = "SearchQuery";

        private string searchQuery = "";

        /// <summary>
        /// Sets and gets the SearchQuery property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SearchQuery
        {
            get
            {
                return searchQuery;
            }

            set
            {
                if (searchQuery == value)
                {
                    return;
                }

                searchQuery = value;
                RaisePropertyChanged(SearchQueryPropertyName);
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
                        int index = 0;
                        foreach (var song in SearchResults)
                        {
                            if (song.SongId == item.SongId) break;
                            index++;
                        }
                        ApplicationSettingsHelper.SaveSongIndex(index);
                        Library.Current.SetNowPlayingList(SearchResults);
                        
                        navigationService.NavigateTo(ViewNames.NowPlayingView, item.SongId);
                    }));
            }
        }

        private RelayCommand searchClick;

        /// <summary>
        /// Gets the SearchClick.
        /// </summary>
        public RelayCommand SearchClick
        {
            get
            {
                return searchClick
                    ?? (searchClick = new RelayCommand(
                    () =>
                    {
                        Search(searchQuery);
                    }));
            }
        }

        public async void Search(string value)
        {
            SearchResults = await DatabaseManager.SearchSongs(value);
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {
        }

        public void Deactivate(Dictionary<string, object> state)
        {
        }

    }
}