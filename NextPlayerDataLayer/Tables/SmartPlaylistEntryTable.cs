﻿using SQLite;

namespace NextPlayerDataLayer.Tables
{
    [Table("SmartPlaylistEntryTable")]
    class SmartPlaylistEntryTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int PlaylistId { get; set; }
        public string Comparison { get; set; }
        public string Item { get; set; }
        public string Value { get; set; }
    }
}
