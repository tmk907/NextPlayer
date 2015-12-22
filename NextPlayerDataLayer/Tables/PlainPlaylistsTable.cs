﻿using SQLite;

namespace NextPlayerDataLayer.Tables
{
    [Table("PlainPlaylistsTable")]
    class PlainPlaylistsTable
    {
        [PrimaryKey, AutoIncrement]
        public int PlainPlaylistId { get; set; }
        public string Name { get; set; }
    }
}
