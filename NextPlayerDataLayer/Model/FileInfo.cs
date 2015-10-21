using System;

namespace NextPlayerDataLayer.Model
{
    public class FileInfo
    {
        public string FilePath { get; set; }
        public int Bitrate { get; set; }
        public int FileSize { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string AlbumArtist { get; set; }
        public string Composer { get; set; }
        //public string Conductor { get; set; }
        public int TrackNumber { get; set; }
        public int Year { get; set; }
        public int Rating { get; set; }
        public int PlayCount { get; set; }
        public DateTime LastTimePlayed { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
