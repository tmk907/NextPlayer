using NextPlayerDataLayer.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace NextPlayerDataLayer.Helpers
{
    public enum RepeatEnum
    {
        NoRepeat,
        RepeatOnce,
        RepeatPlaylist
    }

    public class Repeat
    {
        public static readonly string NoRepeat = "\uE17e\uE1cd";
        public static readonly string RepeatOnce = "\uE17e\uE1cc";
        public static readonly string RepeatPlaylist = "\uE17e\uE1cd";

        public static RepeatEnum Next(RepeatEnum e)
        {
            switch (e)
            {
                case RepeatEnum.NoRepeat:
                    return RepeatEnum.RepeatOnce;
                case RepeatEnum.RepeatOnce:
                    return RepeatEnum.RepeatPlaylist;
                case RepeatEnum.RepeatPlaylist:
                    return RepeatEnum.NoRepeat;
                default:
                    return RepeatEnum.NoRepeat;
            }
        }

        public static RepeatEnum Change(RepeatEnum e)
        {
            RepeatEnum newEnum = Next(e);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.Repeat, newEnum.ToString());
            return newEnum;
        }

        public static RepeatEnum CurrenState()
        {
            RepeatEnum repeat;
            object o = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.Repeat);
            if (o != null)
            {
               repeat = (RepeatEnum)Enum.Parse(typeof(RepeatEnum), (string) o, true);
            }
            else
            {
                repeat = RepeatEnum.NoRepeat;
            }
            return repeat;
        }

        public static SolidColorBrush CurrentStateColor(RepeatEnum e)
        {
            switch (e)
            {
                case RepeatEnum.NoRepeat:
                    return new SolidColorBrush(Windows.UI.Colors.Gray);
                case RepeatEnum.RepeatOnce:
                    return new SolidColorBrush(Windows.UI.Colors.White);
                case RepeatEnum.RepeatPlaylist:
                    return new SolidColorBrush(Windows.UI.Colors.White);
                default:
                    return new SolidColorBrush(Windows.UI.Colors.Gray);
            }
        }

        public static string CurrentStateContent(RepeatEnum e)
        {
            switch (e)
            {
                case RepeatEnum.NoRepeat:
                    return NoRepeat;
                case RepeatEnum.RepeatOnce:
                    return RepeatOnce;
                case RepeatEnum.RepeatPlaylist:
                    return RepeatPlaylist;
                default:
                    return NoRepeat;
            }
        }
    }
}
