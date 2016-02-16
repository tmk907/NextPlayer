using NextPlayerDataLayer.Diagnostics;
using NextPlayerDataLayer.Model;
using System;
using System.IO;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;

namespace NextPlayerDataLayer.Services
{
    public class FileTagsUpdater
    {
        public async Task UpdateFileTags(SongData songData)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(songData.Path);

                using (Stream fileReadStream = await file.OpenStreamForReadAsync())
                {
                    using (Stream fileWriteStream = await file.OpenStreamForWriteAsync())
                    {
                        using (File tagFile = TagLib.File.Create(new OwnFileAbstraction(file.Name, fileReadStream, fileWriteStream)))
                        {
                            tagFile.Tag.AlbumArtists = null;
                            tagFile.Tag.Composers = null;
                            tagFile.Tag.Performers = null;
                            tagFile.Tag.Album = songData.Tag.Album;
                            tagFile.Tag.AlbumArtists = new string[] { songData.Tag.AlbumArtist };
                            tagFile.Tag.Performers = songData.Tag.Artists.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            tagFile.Tag.Comment = songData.Tag.Comment;
                            tagFile.Tag.Composers = songData.Tag.Composers.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            tagFile.Tag.Conductor = songData.Tag.Conductor;
                            tagFile.Tag.Disc = (uint)songData.Tag.Disc;
                            tagFile.Tag.Genres = new string[] { songData.Tag.Genre };
                            tagFile.Tag.Title = songData.Tag.Title;
                            tagFile.Tag.Track = (uint)songData.Tag.Track;
                            tagFile.Tag.Year = (uint)songData.Tag.Year;
                            tagFile.Save();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Save("UpdateFileTags() " + Environment.NewLine + songData.Path + Environment.NewLine + ex.Message);
                Logger.SaveToFile();
            }
        }

        public async Task UpdateLyrics(int songId, string lyrics)
        {
            string path = DatabaseManager.GetFileInfo(songId).FilePath;
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                using (Stream fileReadStream = await file.OpenStreamForReadAsync())
                {
                    using (Stream fileWriteStream = await file.OpenStreamForWriteAsync())
                    {
                        using (File tagFile = TagLib.File.Create(new OwnFileAbstraction(file.Name, fileReadStream, fileWriteStream)))
                        {
                            tagFile.Tag.Lyrics = lyrics;
                            tagFile.Save();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Save("UpdateLyrics() " + Environment.NewLine + path + Environment.NewLine + ex.Message);
                Logger.SaveToFile();
            }
        }

        public async Task UpdateRating(int songId, int rating)
        {
            string path = DatabaseManager.GetFileInfo(songId).FilePath;
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                using (Stream fileReadStream = await file.OpenStreamForReadAsync())
                {
                    using (Stream fileWriteStream = await file.OpenStreamForWriteAsync())
                    {
                        using (File tagFile = TagLib.File.Create(new OwnFileAbstraction(file.Name, fileReadStream, fileWriteStream)))
                        {
                            if (tagFile.TagTypes.ToString().Contains(TagTypes.Id3v2.ToString()))
                            {
                                Tag tags = tagFile.GetTag(TagTypes.Id3v2);

                                TagLib.Id3v2.PopularimeterFrame pop = TagLib.Id3v2.PopularimeterFrame.Get((TagLib.Id3v2.Tag)tags, "Windows Media Player 9 Series", true);
                                if (pop != null)
                                {
                                    if (rating == 5) pop.Rating = 255;
                                    else if (rating == 4) pop.Rating = 196;
                                    else if (rating == 3) pop.Rating = 128;
                                    else if (rating == 2) pop.Rating = 64;
                                    else if (rating == 1) pop.Rating = 1;
                                    else if (rating == 0) pop.Rating = 0;

                                    TagLib.Id3v2.Tag.DefaultVersion = 3;
                                    TagLib.Id3v2.Tag.ForceDefaultVersion = true;
                                    tagFile.Save();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Save("UpdateRating() " + Environment.NewLine + path + Environment.NewLine + ex.Message);
                Logger.SaveToFile();
            }
        }
    }
}
