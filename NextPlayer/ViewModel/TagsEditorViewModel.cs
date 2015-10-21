using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using NextPlayerDataLayer.Model;
using NextPlayerDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayer.ViewModel
{
    public class TagsEditorViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private int songId;
        private SongData songData;

        public TagsEditorViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        /// <summary>
        /// The <see cref="TagData" /> property's name.
        /// </summary>
        public const string TagDataPropertyName = "TagData";

        private Tags tagData = new Tags();

        /// <summary>
        /// Sets and gets the TagData property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Tags TagData
        {
            get
            {
                return tagData;
            }

            set
            {
                if (tagData == value)
                {
                    return;
                }

                tagData = value;
                RaisePropertyChanged(TagDataPropertyName);
            }
        }

        private RelayCommand save;

        /// <summary>
        /// Gets the Save.
        /// </summary>
        public RelayCommand Save
        {
            get
            {
                return save
                    ?? (save = new RelayCommand(
                    () =>
                    {
                        TagData.FirstArtist = GetFirst(tagData.Artists);
                        TagData.FirstComposer = GetFirst(tagData.Composers);
                        songData.Tag = TagData;
                        DatabaseManager.UpdateSongData(songData, songId);
                        MediaImport.UpdateFileTags(songData);
                        navigationService.GoBack();
                    }));
            }
        }

        private RelayCommand cancel;

        /// <summary>
        /// Gets the Cancel.
        /// </summary>
        public RelayCommand Cancel
        {
            get
            {
                return cancel
                    ?? (cancel = new RelayCommand(
                    () =>
                    {
                        navigationService.GoBack();
                    }));
            }
        }

        private string GetFirst(string text)
        {
            if (text.IndexOf(';') > 0)
            {
                return text.Substring(0, text.IndexOf(';'));
            }
            return text;
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {
            songId = -1;
            songData = new SongData();
            if (parameter != null)
            {
                songId = Int32.Parse(parameter.ToString());
                songData = DatabaseManager.SelectSongData(songId);
            }
            TagData = songData.Tag;
        }

        public void Deactivate(Dictionary<string, object> state)
        {
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }
    }
}
