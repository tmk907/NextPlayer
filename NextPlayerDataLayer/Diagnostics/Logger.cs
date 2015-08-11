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

        private static string temp = "";

        public async static void SaveToFile()
        {
            string content = temp;
            temp = "";
            // saves the string 'content' to a file 'filename' in the app's local storage folder
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content.ToCharArray());

            // create a file with the given filename in the local folder; replace any existing file with the same name
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);

            await FileIO.AppendTextAsync(file, content);
        }

        public static void Save(string data)
        {
            temp += DateTime.Now.TimeOfDay.ToString() + " " + data + "&";
        }

        public async static Task<string> ReadAll()
        {
            string text;
            // reads the contents of file 'filename' in the app's local storage folder and returns it as a string

            // access the local folder
            StorageFolder local = ApplicationData.Current.LocalFolder;
            // open the file 'filename' for reading
            try
            {
                Stream stream = await local.OpenStreamForReadAsync(filename);

                // copy the file contents into the string 'text'
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

        public async static void Clear()
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
        }
    }
}
