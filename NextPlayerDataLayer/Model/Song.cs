﻿using System;

namespace NextPlayerDataLayer.Model
{
    public class SongData
    {
        public int SongId { get; set; }
        public string Filename { get; set; }
        public ulong FileSize { get; set; }
        public string Path { get; set; }
        
        public uint Bitrate { get; set; }
        public TimeSpan Duration { get; set; }
        
        public DateTime LastPlayed { get; set; }
        public DateTime DateAdded { get; set; }
        public uint PlayCount { get; set; }

        public int IsAvailable { get; set; }

        public Tags Tag { get; set; }

        public void AddTag(Tags t)
        {
            this.Tag = t;
        }
        public SongData()
        {
            this.Tag = new Tags();
        }
    }
}
