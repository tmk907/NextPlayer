using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerDataLayer.Model
{
    public class SongData
    {
        public int SongId { get; set; }
        public string Album { get; set; }
        public string AlbumArtist { get; set; }
        public string Artist { get; set; }
        public uint Bitrate { get; set; }
        public TimeSpan Duration { get; set; }
        public string Genre { get; set; }
        public string Publisher { get; set; }
        public uint Rating { get; set; }
        public string Subtitle { get; set; }
        public string Title { get; set; }
        public uint TrackNumber { get; set; }
        public uint Year { get; set; }

        public string Path { get; set; }
        public string Filename { get; set; }
        public ulong FileSize { get; set; }
        public DateTime LastPlayed { get; set; }
        public DateTime DateAdded { get; set; }
        public uint PlayCount { get; set; }

        public int IsAvailable { get; set; }

        public string Lyrics { get; set; }
    }
}
