using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerDataLayer.Model
{
    public class ArtistItem: INotifyPropertyChanged
    {
        private string artist;
        public string Artist
        {
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
        private int albumsNumber;
        public int AlbumsNumber
        {
            get
            {
                return albumsNumber;
            }
            set
            {
                if (value != albumsNumber)
                {
                    albumsNumber = value;
                    onPropertyChanged(this, "AlbumsNumber");
                }
            }
        }
        private int songsNumber;
        public int SongsNumber
        {
            get
            {
                return songsNumber;
            }
            set
            {
                if (value != songsNumber)
                {
                    songsNumber = value;
                    onPropertyChanged(this, "SongsNumber");
                }
            }
        }

        public ArtistItem()
        {
            
            artist = "Unknown Artist";
            duration = TimeSpan.Zero;
            songsNumber = 0;
            albumsNumber = 0;
        }

        public ArtistItem(int albumsnumber, string artist, TimeSpan duration, int songsnumber)
        {
            this.albumsNumber = albumsnumber;
            this.artist = artist;
            this.duration = duration;
            this.songsNumber = songsnumber;
        }

        public override string ToString()
        {
            return artist;
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
