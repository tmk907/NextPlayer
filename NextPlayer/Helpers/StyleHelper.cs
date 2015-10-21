using NextPlayerDataLayer.Constants;
using NextPlayerDataLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NextPlayer.Helpers
{
    public class StyleHelper
    {
        public static void ChangeMainPageButtonsBackground()
        {
            bool isImageSet = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IsBGImageSet);
            bool isPhoneAccent = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IsPhoneAccentSet);
            Windows.UI.Color color;
            if (isImageSet)
            {
                byte a = byte.Parse("D0", System.Globalization.NumberStyles.HexNumber);
                if (isPhoneAccent)
                {
                    color = ((SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"]).Color;
                    color.A = a;
                }
                else
                {
                    string hexColor = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AppAccent) as string;
                    byte r = byte.Parse(hexColor.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                    byte g = byte.Parse(hexColor.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                    byte b = byte.Parse(hexColor.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
                    color = Windows.UI.Color.FromArgb(a, r, g, b);
                }
                ((SolidColorBrush)App.Current.Resources["MainPageButtonsBackground"]).Color = color;
            }
            else
            {
                if (isPhoneAccent)
                {
                    App.Current.Resources["MainPageButtonsBackground"] = ((SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"]);
                }
                else
                {
                    string hexColor = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AppAccent) as string;
                    byte a = byte.Parse(hexColor.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                    byte r = byte.Parse(hexColor.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                    byte g = byte.Parse(hexColor.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                    byte b = byte.Parse(hexColor.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
                    color = Windows.UI.Color.FromArgb(a, r, g, b);
                    ((SolidColorBrush)App.Current.Resources["MainPageButtonsBackground"]).Color = color;
                }
            }
        }

        public static void EnableBGImage()
        {
            //#80161616 = 128,22,22,22 
            //#64161616 = 100,22,22,22 
            //((SolidColorBrush)App.Current.Resources["UserListFontColor"]).Color = Windows.UI.Colors.White;
            string path = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.BackgroundImagePath) as string;
            if (path != null && path != "")
            {
                ((ImageBrush)App.Current.Resources["BgImage"]).ImageSource = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                Windows.UI.Color color = Windows.UI.Color.FromArgb(128, 22, 22, 22);
                ((SolidColorBrush)App.Current.Resources["TransparentBGImageColor"]).Color = color;
                color = Windows.UI.Color.FromArgb(100, 22, 22, 22);
                ((SolidColorBrush)App.Current.Resources["TransparentMainPageBGImageColor"]).Color = color;
            }
            else
            {

            }

            if (App.Current.RequestedTheme == ApplicationTheme.Light)
            {
                App.Current.Resources["UserListFontColor"] = new SolidColorBrush(Windows.UI.Colors.White);
            }
            ChangeAlbumViewTransparency();
        }

        public static void DisableBGImage()
        {
            ((ImageBrush)App.Current.Resources["BgImage"]).ImageSource = new BitmapImage();
            Windows.UI.Color color = Windows.UI.Color.FromArgb(0, 0, 0, 0);
            ((SolidColorBrush)App.Current.Resources["TransparentBGImageColor"]).Color = color;
            ((SolidColorBrush)App.Current.Resources["TransparentMainPageBGImageColor"]).Color = color;
            if (App.Current.RequestedTheme == ApplicationTheme.Light)
            {
                App.Current.Resources["UserListFontColor"] = new SolidColorBrush(Windows.UI.Colors.Black);
            }
            ChangeAlbumViewTransparency();
        }

        public static void ChangeAlbumViewTransparency()
        {
            bool cover = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.ShowCoverAsBackground);
            bool image = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IsBGImageSet);
            if (cover || image)
            {
                ((SolidColorBrush)App.Current.Resources["TransparentAlbumInfoColor"]).Color = Windows.UI.Color.FromArgb(100, 22, 22, 22);
                ((SolidColorBrush)App.Current.Resources["TransparentAlbumBGImageColor"]).Color = Windows.UI.Color.FromArgb(128, 22, 22, 22);
            }
            else
            {
                ((SolidColorBrush)App.Current.Resources["TransparentAlbumInfoColor"]).Color = Windows.UI.Color.FromArgb(0, 0, 0, 0);
                ((SolidColorBrush)App.Current.Resources["TransparentAlbumBGImageColor"]).Color = Windows.UI.Color.FromArgb(0, 0, 0, 0);
            }
        }
    }
}
