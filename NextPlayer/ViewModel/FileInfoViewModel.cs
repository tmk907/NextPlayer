using NextPlayer.Constants;
using NextPlayerDataLayer.Model;
using NextPlayerDataLayer.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayer.ViewModel
{
    public class FileInfoViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private int songId;

        public FileInfoViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        /// <summary>
        /// The <see cref="Song" /> property's name.
        /// </summary>
        public const string SongPropertyName = "Song";

        private SongData song = new SongData();

        /// <summary>
        /// Sets and gets the Song property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public SongData Song
        {
            get
            {
                return song;
            }

            set
            {
                if (song == value)
                {
                    return;
                }

                song = value;
                RaisePropertyChanged(SongPropertyName);
            }
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {
            songId = -1;
            song = new SongData();
            if (parameter != null)
            {
                songId = Int32.Parse(parameter.ToString());
                AddFileSize(DatabaseManager.SelectSongData(songId));
            }
        }
        private async Task AddFileSize(SongData s)
        {
            try
            {
                Windows.Storage.IStorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(s.Path);
                s.FileSize = file.OpenAsync(Windows.Storage.FileAccessMode.Read).AsTask().Result.Size;
            }
            catch(Exception ex)
            {
                App.TelemetryClient.TrackTrace("AddFileSize" + Environment.NewLine + ex.Message, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
            }
            Song = s;
        }

        public void Deactivate(Dictionary<string, object> state)
        {
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }
    }
}