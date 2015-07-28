using NextPlayerDataLayer.Common;
using NextPlayerDataLayer.Model;
using NextPlayerDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerDataLayer.Helpers
{
    public class PerfTests
    {
        public PerfTests()
        {

        }

        public async void Run()
        {
            //var watch = Stopwatch.StartNew();
            //DatabaseManager.GetSongItems();
            //watch.Stop();
            //long t1 = watch.ElapsedMilliseconds;

            //watch.Restart();
            //await DatabaseManager.GetSongItemsAsync();
            //watch.Stop();
            //long t2 = watch.ElapsedMilliseconds;

            //watch.Restart();
            //DatabaseManager.GetSongItemsFromGenre("Unknown");
            //watch.Stop();
            //long t3 = watch.ElapsedMilliseconds;

            //watch.Restart();
            //await DatabaseManager.GetSongItemsFromGenreAsync("Unknown");
            //watch.Stop();
            //long t4 = watch.ElapsedMilliseconds;
            //var w = Stopwatch.StartNew();
            //DatabaseManager.ConnectionDb();
            //w.Stop();
            //long q1 = w.ElapsedMilliseconds;
            //w.Restart();
            //DatabaseManager.AsyncConnectionDb();
            //w.Stop();
            //long q2 = w.ElapsedMilliseconds;


            var watch = Stopwatch.StartNew();
            var a = await DatabaseManager.GetSongItemsAsync();
            var b = Grouped.CreateGrouped<SongItem>(a, x => x.Title);
            watch.Stop();
            long t1 = watch.ElapsedMilliseconds;
            watch.Restart();
            var c = DatabaseManager.GetSongItems();
            var d = Grouped.CreateGrouped<SongItem>(c, x => x.Title);
            watch.Stop();
            long t2 = watch.ElapsedMilliseconds;
        }

        
    }
}
