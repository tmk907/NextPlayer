using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerDataLayer.Diagnostics
{
    public class Logger
    {
        private const string filename = "log1.txt";
        private const string filenameBG = "logBG1.txt";
        private const string lastfmlog = "lastfmlog.txt";

        private static string temp = "";
        private static string tempBG = "";

        private static bool BGLogON = false;

        public async static void SaveToFile()
        {
            string content = temp;
            temp = "";
            // saves the string 'content' to a file 'filename' in the app's local storage folder
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content.ToCharArray());
            try
            {
                // create a file with the given filename in the local folder; replace any existing file with the same name
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);

                await FileIO.AppendTextAsync(file, content);
            }
            catch (Exception e)
            {
                Logger.Save("error log\n"+content+"\n");
            }
           
        }

        public static void Save(string data)
        {
            temp += DateTime.Now.ToString() + " " + data + "\n"+System.Environment.NewLine;
        }

        
        public async static Task<string> Read()
        {
            string text;
            
            StorageFolder local = ApplicationData.Current.LocalFolder;
            try
            {
                Stream stream = await local.OpenStreamForReadAsync(filename);

                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
            }
            catch(FileNotFoundException e) 
            {
                text = e.Message;
            }
            
            return text;
        }

        public async static Task<string> ReadBG()
        {
            string text;

            StorageFolder local = ApplicationData.Current.LocalFolder;
            try
            {
                Stream stream = await local.OpenStreamForReadAsync(filenameBG);

                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
            }
            catch (FileNotFoundException e)
            {
                text = e.Message;
            }

            return text;
        }

        public async static void ClearAll()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            await ApplicationData.Current.LocalFolder.CreateFileAsync(filenameBG, CreationCollisionOption.ReplaceExisting);
        }

        public async static void SaveToFileBG()
        {
            if (BGLogON)
            {
                string content = tempBG;
                tempBG = "";
                byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content.ToCharArray());
                try
                {
                    StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filenameBG, CreationCollisionOption.OpenIfExists);

                    await FileIO.AppendTextAsync(file, content);
                }
                catch (Exception e)
                {
                    Logger.SaveBG("error log\n" + content + "\n" + e.Message);
                }
            }
        }
        public static void SaveBG(string data)
        {
            if (BGLogON)
            {
                tempBG += DateTime.Now.ToString() + " " + data + "\n" + System.Environment.NewLine;
            }
        }


        public async static void SaveLastFm(string data)
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(lastfmlog, CreationCollisionOption.OpenIfExists);
                await FileIO.AppendTextAsync(file, DateTime.Now.ToString() + Environment.NewLine + data + Environment.NewLine);
            }
            catch (Exception e)
            {
                
            }
        }

        public async static Task<string> ReadLastFm()
        {
            string text = "";
            StorageFolder local = ApplicationData.Current.LocalFolder;
            try
            {
                Stream stream = await local.OpenStreamForReadAsync(lastfmlog);
                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
            }
            catch (FileNotFoundException e)
            {
                
            }

            return text;
        }

        public async static void ClearLastFm()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync(lastfmlog, CreationCollisionOption.ReplaceExisting);
        }
    }
}
