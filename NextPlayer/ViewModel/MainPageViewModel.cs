using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using NextPlayer.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayer.ViewModel
{
    public class MainPageViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;

        public MainPageViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        #region Commands
        private RelayCommand goToNowPlayingListPage;

        /// <summary>
        /// Gets the GoToNowPlayingListPage.
        /// </summary>
        public RelayCommand GoToNowPlayingListPage
        {
            get
            {
                return goToNowPlayingListPage
                    ?? (goToNowPlayingListPage = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo(ViewNames.PlaylistView);
                    }));
            }
        }

        private RelayCommand goToSongsPage;

        /// <summary>
        /// Gets the GoToSongsPage.
        /// </summary>
        public RelayCommand GoToSongsPage
        {
            get
            {
                return goToSongsPage
                    ?? (goToSongsPage = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo(ViewNames.SongsView);
                    }));
            }
        }

        private RelayCommand goToAlbumsPage;

        /// <summary>
        /// Gets the GoToAlbumsPage.
        /// </summary>
        public RelayCommand GoToAlbumsPage
        {
            get
            {
                return goToAlbumsPage
                    ?? (goToAlbumsPage = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo(ViewNames.AlbumsView);
                    }));
            }
        }

        private RelayCommand goToArtistsPage;

        /// <summary>
        /// Gets the GoToArtistsPage.
        /// </summary>
        public RelayCommand GoToArtistsPage
        {
            get
            {
                return goToArtistsPage
                    ?? (goToArtistsPage = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo(ViewNames.ArtistsView);
                    }));
            }
        }

        private RelayCommand goToFoldersPage;

        /// <summary>
        /// Gets the GoToFoldersPage.
        /// </summary>
        public RelayCommand GoToFoldersPage
        {
            get
            {
                return goToFoldersPage
                    ?? (goToFoldersPage = new RelayCommand(
                    () =>
                    {
                        NextPlayerDataLayer.Helpers.PerfTests s = new NextPlayerDataLayer.Helpers.PerfTests();
                        s.Run();
                        //navigationService.NavigateTo(ViewNames.NowPlayingView);
                    }));
            }
        }

        private RelayCommand goToGenresPage;

        /// <summary>
        /// Gets the GoToGenresPage.
        /// </summary>
        public RelayCommand GoToGenresPage
        {
            get
            {
                return goToGenresPage
                    ?? (goToGenresPage = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo(ViewNames.GenresView);
                    }));
            }
        }

        private RelayCommand goToPlaylistsPage;

        /// <summary>
        /// Gets the GoToPlaylistsPage.
        /// </summary>
        public RelayCommand GoToPlaylistsPage
        {
            get
            {
                return goToPlaylistsPage
                    ?? (goToPlaylistsPage = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo(ViewNames.PlaylistsView);
                    }));
            }
        }

        private RelayCommand goToSettingsPage;

        /// <summary>
        /// Gets the GoToSettingsPage.
        /// </summary>
        public RelayCommand GoToSettingsPage
        {
            get
            {
                return goToSettingsPage
                    ?? (goToSettingsPage = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo(ViewNames.SettingsView);
                    }));
            }
        }

        #endregion

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
