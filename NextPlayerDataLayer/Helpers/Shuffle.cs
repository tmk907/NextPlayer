using NextPlayerDataLayer.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace NextPlayerDataLayer.Helpers
{
    public class Shuffle
    {
        public static readonly string ShuffleButtonContent = "&#xE17e;&#xE14b;";

        public static bool Change(bool s)
        {
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.Shuffle, !s);
            return !s;
        }

        public static bool CurrentState()
        {
            bool b;
            object s = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.Shuffle);
            if (s != null)
            {
                b = (bool)s;
            }
            else
            {
                b = false;
            }
            return b;
        }

        public static string CurrentStateString()
        {
            if (CurrentState())
            {
                return "ShuffleOn";
            }
            else
            {
                return "ShuffleOff";
            }
        }
        public static SolidColorBrush CurrentStateColor()
        {
            if (CurrentState())
            {
                return new SolidColorBrush(Windows.UI.Colors.White);
            }
            else
            {
                return new SolidColorBrush(Windows.UI.Colors.Gray);
            }
        }
    }
}
