using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerDataLayer.Model
{
    public class FolderItem
    {
        public ObservableCollection<SongItem> SongsList { get; set; }
        public string FolderName { get; set; }
        public int SongsNumber { get; set; }
        public int TotalTime { get; set; }
    }
}
