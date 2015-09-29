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

namespace NextPlayerUniversal.ViewModel
{
    public class SettingsViewModel :ViewModelBase, INavigable
    {
        private INavigationService navigationService;

        public SettingsViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {            
        }

        public void Deactivate(Dictionary<string, object> state)
        {
        }

    }
}