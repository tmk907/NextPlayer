﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUniversal.Diagnostics
{
    public class Logger
    {
        private const string filename = "log1.txt";
        private const string filenameBG = "logBG1.txt";

        private static string temp = "";
        private static string tempBG = "";

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

        
        public async static Task<string> ReadAll()
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

        public async static void Clear()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            await ApplicationData.Current.LocalFolder.CreateFileAsync(filenameBG, CreationCollisionOption.ReplaceExisting);
        }

        public async static void SaveToFileBG()
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
                Logger.SaveBG("error log\n"+content+"\n");
            }

        }
        public static void SaveBG(string data)
        {
            tempBG += DateTime.Now.ToString() + " " + data + "\n" + System.Environment.NewLine;
        }

    }
}
