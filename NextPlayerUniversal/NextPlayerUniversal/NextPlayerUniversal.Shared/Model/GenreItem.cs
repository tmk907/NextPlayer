using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUniversal.Model
{
    public class GenreItem : INotifyPropertyChanged
    {
        private string genre;
        public string Genre { get { return genre; } }
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
        private TimeSpan duration;
        public TimeSpan Duration { get { return duration; } }

        public GenreItem()
        {
            this.duration = TimeSpan.Zero;
            this.genre = "Unknown Genre";
            this.songsNumber = 0;
        }

        public GenreItem(TimeSpan duration, string genre, int songsnumber)
        {
            this.duration = duration;
            this.genre = genre;
            this.songsNumber = songsnumber;
        }

        public override string ToString()
        {
            return "genre|" + genre;
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
