﻿using System.ComponentModel;

namespace NextPlayerDataLayer.Model
{
    public class FolderItem : INotifyPropertyChanged
    {
        private string folder;
        public string Folder { get { return folder; } }
        private string directory;
        public string Directory { get { return directory; } }
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
        public FolderItem()
        {
            this.folder = "Unknown Folder";
            this.songsNumber = 0;
        }

        public FolderItem(string folder, string directory, int songsnumber)
        {
            this.folder = folder;
            this.directory = directory;
            this.songsNumber = songsnumber;
        }

        public override string ToString()
        {
            return "folder|" + directory;
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
