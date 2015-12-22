using NextPlayerDataLayer.Model;
using NextPlayerDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerDataLayer.Helpers
{
    public class SaveLater
    {
        private List<SongData> songs;
        private List<Tuple<int, int>> ratings;

        private static SaveLater current = null;
        private SaveLater()
        {
            songs = new List<SongData>();
            ratings = new List<Tuple<int, int>>();
            object r = ApplicationSettingsHelper.ReadResetSettingsValue("savelaterratings");
            if (r != null)
            {
                string[] a = r.ToString().Split(new char[]{ '|' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < a.Length; i++)
                {
                    ratings.Add(new Tuple<int, int>(Int32.Parse(a[2 * i]), Int32.Parse(a[2 * i + 1])));
                }
            }
            object t = ApplicationSettingsHelper.ReadResetSettingsValue("savelatertags");
            if (t != null)
            {
                string[] a = t.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach(var item in a)
                {
                    songs.Add(DatabaseManager.SelectSongData(Int32.Parse(item)));
                }
            }
        }

        public static SaveLater Current
        {
            get
            {

                if (current == null)
                {
                    current = new SaveLater();
                }
                return current;
            }
        }

        public async Task SaveTagsNow()
        {
            foreach(var song in songs)
            {
                var s = Library.Current.GetCurrentPlayingSong();
                if (s != null && s.SongId != song.SongId)
                {
                    await SaveTags(song);
                }
            }
            //! some data can be lost
            songs = new List<SongData>();
            ApplicationSettingsHelper.ReadResetSettingsValue("savelatertags");
        }

        public async Task SaveRatingsNow()
        {
            foreach (var item in ratings)
            {
                var song = Library.Current.GetCurrentPlayingSong();
                if (song != null && song.SongId != item.Item1)
                {
                    await SaveRating(item.Item1, item.Item2);
                }
            }
            //! some data can be lost
            ratings = new List<Tuple<int, int>>();
            ApplicationSettingsHelper.ReadResetSettingsValue("savelaterratings");
        }

        public void SaveTagsLater(SongData song)
        {
            songs.Add(song);
            ApplicationSettingsHelper.SaveSettingsValue("savelatertags", (ApplicationSettingsHelper.ReadSettingsValue("savelatertags") ?? "").ToString() + song.SongId + "|");
        }

        public void SaveRatingLater(int songId, int rating)
        {
            ratings.Add(new Tuple<int, int>(songId, rating));
            ApplicationSettingsHelper.SaveSettingsValue("savelaterratings", (ApplicationSettingsHelper.ReadSettingsValue("savelaterratings") ?? "").ToString() + songId + "|" + rating + "|");
        }

        private async Task SaveTags(SongData song)
        {
            await MediaImport.UpdateFileTags(song);
        }

        private async Task SaveRating(int songId, int rating)
        {
            await MediaImport.UpdateRating(songId, rating);
        }

    }
}
