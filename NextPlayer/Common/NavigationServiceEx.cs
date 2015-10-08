using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayer.Common
{
    public interface INavigationServiceEx : INavigationService
    {
        bool CanGoBack { get; }
     
        bool RemoveBackEntry();       
    }

    public class NavigationServiceEx : NavigationService, INavigationServiceEx
    {
        public bool CanGoBack
        {
            get
            {
                var frame = this.GetMainFrame();
                if (frame != null)
                    return frame.CanGoBack;

                return false;
            }
        }

        public bool RemoveBackEntry()
        {
            var frame = this.GetMainFrame();
            if (frame.CanGoBack)
            {
                frame.BackStack.RemoveAt(frame.BackStackDepth - 1);
                return true;
            }

            return false;
        }

        private Frame GetMainFrame()
        {
            return (Frame)Window.Current.Content;
        }
    }
}
