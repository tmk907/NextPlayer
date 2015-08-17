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

        public FileInfoViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }
        #region Properties
        /// <summary>
        /// The <see cref="FilePath" /> property's name.
        /// </summary>
        public const string FilePathPropertyName = "FilePath";

        private string filePath = "";

        /// <summary>
        /// Sets and gets the FilePath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string FilePath
        {
            get
            {
                return filePath;
            }

            set
            {
                if (filePath == value)
                {
                    return;
                }

                filePath = value;
                RaisePropertyChanged(FilePathPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Bitrate" /> property's name.
        /// </summary>
        public const string BitratePropertyName = "Bitrate";

        private string bitrate = "";

        /// <summary>
        /// Sets and gets the Bitrate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Bitrate
        {
            get
            {
                return bitrate;
            }

            set
            {
                if (bitrate == value)
                {
                    return;
                }

                bitrate = value;
                RaisePropertyChanged(BitratePropertyName);
            }
        }



        /// <summary>
        /// The <see cref="FileSize" /> property's name.
        /// </summary>
        public const string FileSizePropertyName = "FileSize";

        private string fileSize = "";

        /// <summary>
        /// Sets and gets the FileSize property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string FileSize
        {
            get
            {
                return fileSize;
            }

            set
            {
                if (fileSize == value)
                {
                    return;
                }

                fileSize = value;
                RaisePropertyChanged(FileSizePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Title" /> property's name.
        /// </summary>
        public const string TitlePropertyName = "Title";

        private string title = "";

        /// <summary>
        /// Sets and gets the Title property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                if (title == value)
                {
                    return;
                }

                title = value;
                RaisePropertyChanged(TitlePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Album" /> property's name.
        /// </summary>
        public const string AlbumPropertyName = "Album";

        private string album = "";

        /// <summary>
        /// Sets and gets the Album property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Album
        {
            get
            {
                return album;
            }

            set
            {
                if (album == value)
                {
                    return;
                }

                album = value;
                RaisePropertyChanged(AlbumPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="Artist" /> property's name.
        /// </summary>
        public const string ArtistPropertyName = "Artist";

        private string artist = "";

        /// <summary>
        /// Sets and gets the Artist property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Artist
        {
            get
            {
                return artist;
            }

            set
            {
                if (artist == value)
                {
                    return;
                }

                artist = value;
                RaisePropertyChanged(ArtistPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="TrackNumber" /> property's name.
        /// </summary>
        public const string TrackNumberPropertyName = "TrackNumber";

        private string trackNumber = "";

        /// <summary>
        /// Sets and gets the TrackNumber property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string TrackNumber
        {
            get
            {
                return trackNumber;
            }

            set
            {
                if (trackNumber == value)
                {
                    return;
                }

                trackNumber = value;
                RaisePropertyChanged(TrackNumberPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Year" /> property's name.
        /// </summary>
        public const string YearPropertyName = "Year";

        private string year = "";

        /// <summary>
        /// Sets and gets the Year property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Year
        {
            get
            {
                return year;
            }

            set
            {
                if (year == value)
                {
                    return;
                }

                year = value;
                RaisePropertyChanged(YearPropertyName);
            }
        }
        
        /// <summary>
        /// The <see cref="Rating" /> property's name.
        /// </summary>
        public const string RatingPropertyName = "Rating";

        private string rating = "";

        /// <summary>
        /// Sets and gets the Rating property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Rating
        {
            get
            {
                return rating;
            }

            set
            {
                if (rating == value)
                {
                    return;
                }

                rating = value;
                RaisePropertyChanged(RatingPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="PlayCount" /> property's name.
        /// </summary>
        public const string PlayCountPropertyName = "PlayCount";

        private string playCount = "";

        /// <summary>
        /// Sets and gets the PlayCount property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string PlayCount
        {
            get
            {
                return playCount;
            }

            set
            {
                if (playCount == value)
                {
                    return;
                }

                playCount = value;
                RaisePropertyChanged(PlayCountPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="LastTimePlayed" /> property's name.
        /// </summary>
        public const string LastTimePlayedPropertyName = "LastTimePlayed";

        private string lastTimePlayed = "";

        /// <summary>
        /// Sets and gets the LastTimePlayed property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string LastTimePlayed
        {
            get
            {
                return lastTimePlayed;
            }

            set
            {
                if (lastTimePlayed == value)
                {
                    return;
                }

                lastTimePlayed = value;
                RaisePropertyChanged(LastTimePlayedPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="DateAdded" /> property's name.
        /// </summary>
        public const string DateAddedPropertyName = "DateAdded";

        private string dateAddeed = "";

        /// <summary>
        /// Sets and gets the DateAdded property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string DateAdded
        {
            get
            {
                return dateAddeed;
            }

            set
            {
                if (dateAddeed == value)
                {
                    return;
                }

                dateAddeed = value;
                RaisePropertyChanged(DateAddedPropertyName);
            }
        }
        #endregion

        /// <summary>
        /// The <see cref="File" /> property's name.
        /// </summary>
        public const string FilePropertyName = "File";

        private FileInfo file = new FileInfo();

        /// <summary>
        /// Sets and gets the File property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public FileInfo File
        {
            get
            {
                return file;
            }

            set
            {
                if (file == value)
                {
                    return;
                }

                file = value;
                RaisePropertyChanged(FilePropertyName);
            }
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {
            if (parameter != null)
            {
                int songId = Int32.Parse(parameter.ToString());
                File = DatabaseManager.GetFileInfo(songId);
            }
        }

        public void Deactivate(Dictionary<string, object> state)
        {
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }
    }
}