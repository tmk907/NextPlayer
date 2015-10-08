using System;
using SQLite;

namespace NextPlayerDataLayer.Tables
{
    [Table("SongsTable")]
    class SongsTable
    {
        [PrimaryKey, AutoIncrement]
        public int SongId { get; set; }
        [Indexed]
        public string Album { get; set; }
        public string AlbumArtist { get; set; }
        [Indexed]
        public string Artist { get; set; }
        public uint Bitrate { get; set; }
        public string Composer { get; set; }
        public TimeSpan Duration { get; set; }
        public string Genre { get; set; }
        public string Performer { get; set; }
        public string Publisher { get; set; }
        public uint Rating { get; set; }
        public string Subtitle { get; set; }
        [Indexed]
        public string Title { get; set; }
        public uint TrackNumber { get; set; }
        public uint Year { get; set; }

        public string Path { get; set; }
        public string Filename { get; set; }
        public long FileSize { get; set; }
        public DateTime LastPlayed { get; set; }
        public DateTime DateAdded { get; set; }
        public uint PlayCount { get; set; }

        [Indexed]
        public int IsAvailable { get; set; }

        public string Lyrics { get; set; }
    }
}
