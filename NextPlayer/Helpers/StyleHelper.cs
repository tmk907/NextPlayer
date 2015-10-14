using NextPlayerDataLayer.Constants;
using NextPlayerDataLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

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
                byte a = byte.Parse("C0", System.Globalization.NumberStyles.HexNumber);
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
    }
}
