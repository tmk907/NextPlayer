using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUniversal.Model
{
    public class NowPlayingSong
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Path { get; set; }
        public int SongId { get; set; }
        public int Position { get; set; }
    }
}
