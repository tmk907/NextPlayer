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


        }

        private static INavigationService CreateNavigationService()
        {
            var navigationService = new NavigationService();
            navigationService.Configure(ViewNames.MainView, typeof(MainPage));
            navigationService.Configure(ViewNames.PlaylistsView, typeof(PlaylistsView));
            navigationService.Configure(ViewNames.SettingsView, typeof(SettingsView));

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

        public PlaylistsViewModel PlaylistsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PlaylistsViewModel>();
            }
        }

        public SettingsViewModel SettingsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingsViewModel>();
            }
        }
    }
}
