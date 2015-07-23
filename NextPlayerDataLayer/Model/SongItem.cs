using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerDataLayer.Model
{
    public class SongItem : INotifyPropertyChanged
    {
        private string album;
        public string Album
        {
            get
            {
                return album;
            }
            set
            {
                if (value != album)
                {
                    album = value;
                    onPropertyChanged(this, "Album");
                }
            }
        }
        private string artist;
        public string Artist {
            get
            {
                return artist;
            }
            set
            {
                if (value != artist)
                {
                    artist = value;
                    onPropertyChanged(this, "Artist");
                }
            }
        }
        private TimeSpan duration;
        public TimeSpan Duration { get { return duration; } }
        private string path;
        public string Path { get { return path; } }
        private int songId;
        public int SongId { get { return songId; } }
        private string title;
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                if (value != title)
                {
                    title = value;
                    onPropertyChanged(this, "Title");
                }
            }
        }
        private int trackNumber;
        public int TrackNumber
        {
            get
            {
                return trackNumber;
            }
            set
            {
                if (value != trackNumber)
                {
                    trackNumber = value;
                    onPropertyChanged(this, "TrackNumber");
                }
            }
        }

        public SongItem()
        {
            title = "Unknown Title";
            artist = "Unknown Artist";
            album = "Unknown Album";
            duration = TimeSpan.Zero;
            this.path = "";
            songId = -1;
        }

        public SongItem(string album, string artist, TimeSpan duration, string path, int songid, string title, int trackumber)
        {
            this.album = album;
            this.artist = artist;
            this.duration = duration;
            this.path = path;
            this.songId = songid;
            this.title = title;
            this.trackNumber = trackumber;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
