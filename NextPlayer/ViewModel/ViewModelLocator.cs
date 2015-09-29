using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using NextPlayer.Constants;
using NextPlayer.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayer.ViewModel
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            //if (ViewModelBase.IsInDesignModeStatic)
            //{
            //    //SimpleIoc.Default.Register<INavigationService, Design.DesignNavigationService>();
            //}
            //else
            //{
                var navigationService = CreateNavigationService();
                SimpleIoc.Default.Register<INavigationService>(() => navigationService);
            //}

            SimpleIoc.Default.Register<IDialogService, DialogService>();

            SimpleIoc.Default.Register<MainPageViewModel>();
            SimpleIoc.Default.Register<PlaylistsViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<GenresViewModel>();
            SimpleIoc.Default.Register<SongsViewModel>();
            SimpleIoc.Default.Register<AlbumsViewModel>();
            SimpleIoc.Default.Register<ArtistsViewModel>();
            SimpleIoc.Default.Register<AlbumViewModel>();
            SimpleIoc.Default.Register<PlaylistViewModel>();
            SimpleIoc.Default.Register<NowPlayingViewModel>();
            SimpleIoc.Default.Register<NewSmartPlaylistViewModel>();
            SimpleIoc.Default.Register<SearchViewModel>();
            SimpleIoc.Default.Register<FoldersViewModel>();
            SimpleIoc.Default.Register<FileInfoViewModel>();
            SimpleIoc.Default.Register<BluetoothShareViewModel>();
        }

        private static INavigationService CreateNavigationService()
        {
            var navigationService = new NavigationService();
            navigationService.Configure(ViewNames.MainView, typeof(MainPage));
            navigationService.Configure(ViewNames.AddToPlaylistView, typeof(AddToPlaylist));
            navigationService.Configure(ViewNames.AlbumsView, typeof(AlbumsView));
            navigationService.Configure(ViewNames.AlbumView, typeof(AlbumView));
            navigationService.Configure(ViewNames.ArtistsView, typeof(ArtistsView));
            navigationService.Configure(ViewNames.GenresView, typeof(GenresView));
            navigationService.Configure(ViewNames.FoldersView, typeof(FoldersView));
            navigationService.Configure(ViewNames.LyricsView, typeof(LyricsView));
            navigationService.Configure(ViewNames.NewSmartPlaylistView, typeof(NewSmartPlaylistView));
            navigationService.Configure(ViewNames.NowPlayingView, typeof(NowPlayingView2));
            //navigationService.Configure(ViewNames.NowPlayingView2, typeof(NowPlayingView2));
            navigationService.Configure(ViewNames.PlaylistsView, typeof(PlaylistsView));
            navigationService.Configure(ViewNames.PlaylistView, typeof(PlaylistView));
            navigationService.Configure(ViewNames.SettingsView, typeof(SettingsView));
            navigationService.Configure(ViewNames.SongsView, typeof(SongView));
            navigationService.Configure(ViewNames.SearchView, typeof(SearchView));
            navigationService.Configure(ViewNames.FileInfoView, typeof(FileInfoView));
            navigationService.Configure(ViewNames.BluetoothShare, typeof(BluetoothShareView));
            // navigationService.Configure("key2", typeof(OtherPage2));
            //HardwareButtons.BackPressed += (sender, args) =>
            //{
            //    navigationService.GoBack();
            //    args.Handled = true;
            //}; 
            return navigationService;
        }

        public MainPageViewModel MainVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainPageViewModel>();
            }
        }

        public NowPlayingViewModel NowPlayingVM
        {       
            get
            {
                return ServiceLocator.Current.GetInstance<NowPlayingViewModel>();
            }
        }

        public SongsViewModel SongsVM
        {
            get 
            { 
                return ServiceLocator.Current.GetInstance<SongsViewModel>();
            }
        }

        public AlbumViewModel AlbumVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AlbumViewModel>();
            }
        }

        public AlbumsViewModel AlbumsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AlbumsViewModel>();
            }
        }

        public ArtistsViewModel ArtistsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ArtistsViewModel>();
            }
        }

        public FileInfoViewModel FileInfoVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<FileInfoViewModel>();
            }
        }

        public FoldersViewModel FoldersVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<FoldersViewModel>(); 
            }
        }

        public GenresViewModel GenresVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<GenresViewModel>();
            }
        }

        public PlaylistViewModel PlaylistVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PlaylistViewModel>();
            }
        }

        public PlaylistsViewModel PlaylistsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PlaylistsViewModel>();
            }
        }

        public SearchViewModel SearchVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SearchViewModel>();
            }
        }

        public SettingsViewModel SettingsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingsViewModel>();
            }
        }

        public NewSmartPlaylistViewModel NewSmartPlaylistVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NewSmartPlaylistViewModel>();
            }
        }

        public BluetoothShareViewModel BluetoothShareVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<BluetoothShareViewModel>();
            }
        }
    }
}
