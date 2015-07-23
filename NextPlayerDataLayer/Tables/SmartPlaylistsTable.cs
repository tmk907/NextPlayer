using System;
using SQLite;

namespace NextPlayerDataLayer.Tables
{
    [Table("SmartPlaylistsTable")]
    class SmartPlaylistsTable
    {
        [PrimaryKey, AutoIncrement]
        public int SmartPlaylistId { get; set; }
        public string Name { get; set; }
        public int SongsNumber { get; set; }
        public string SortBy { get; set; }
    }
}
