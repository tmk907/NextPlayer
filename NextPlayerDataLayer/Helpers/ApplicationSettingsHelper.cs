using NextPlayerDataLayer.Constants;
using System;
using Windows.Storage;

namespace NextPlayerDataLayer.Helpers
{
    public static class ApplicationSettingsHelper
    {
        /// <summary>
        /// Function to read a setting value and clear it after reading it
        /// </summary>
        public static object ReadResetSettingsValue(string key)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                return null;
            }
            else
            {
                var value = ApplicationData.Current.LocalSettings.Values[key];
                ApplicationData.Current.LocalSettings.Values.Remove(key);
                return value;
            }
        }

        public static object ReadSettingsValue(string key)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                return null;
            }
            else
            {
                var value = ApplicationData.Current.LocalSettings.Values[key];
                return value;
            }
        }

        /// <summary>
        /// Save a key value pair in settings. Create if it doesn't exist
        /// </summary>
        public static void SaveSettingsValue(string key, object value)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                ApplicationData.Current.LocalSettings.Values.Add(key, value);
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[key] = value;
            }
        }

        public static void SaveSongIndex(int index)
        {
            int i = -1;
            object value = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.SongIndex);
            if (value != null) Int32.Parse(value.ToString());
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.PrevSongIndex, i);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.SongIndex, index);
        }
    }
}
