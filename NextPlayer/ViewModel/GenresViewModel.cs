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

namespace NextPlayer.ViewModel
{
    public class GenresViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;

        public GenresViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        /// <summary>
        /// The <see cref="Playlists" /> property's name.
        /// </summary>
        public const string GenresPropertyName = "Genres";

        private ObservableCollection<GenreItem> genres = new ObservableCollection<GenreItem>();

        /// <summary>
        /// Sets and gets the Playlists property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<GenreItem> Genres
        {
            get
            {
                if (genres.Count == 0)
                {
                    if (IsInDesignMode)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            genres.Add(new GenreItem(TimeSpan.Zero, i.ToString(), i));
                        }
                    }
                }
                return genres;
            }

            set
            {
                if (genres == value)
                {
                    return;
                }

                genres = value;
                RaisePropertyChanged(GenresPropertyName);
            }
        }

        private RelayCommand<GenreItem> itemClicked;

        /// <summary>
        /// Gets the ItemClicked.
        /// </summary>
        public RelayCommand<GenreItem> ItemClicked
        {
            get
            {
                return itemClicked
                    ?? (itemClicked = new RelayCommand<GenreItem>(
                    item =>
                    {
                        String[] s = new String[2];
                        s[0] = "genre";
                        s[1] = item.Genre
                        navigationService.NavigateTo(ViewNames.PlaylistView, s);
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
                        Genres = DatabaseManager.GetGenreItems();
                    }));
            }
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {
        }

        public void Deactivate(Dictionary<string, object> state)
        {
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }
    }
}