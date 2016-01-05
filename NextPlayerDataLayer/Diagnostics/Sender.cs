using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NextPlayerDataLayer.Diagnostics
{
    public class Sender
    {
        private const string hostname = "http://playerlogs.hol.es/WP/log.php";
        public Sender()
        {
        }
        public async Task Go()
        {
            #if DEBUG
                        return;
            #endif

            try
            {
                await Send();
            }
            catch(Exception ex)
            {

            }
        }

        private async Task Send()
        {
            Uri uri;
            Uri.TryCreate(hostname, UriKind.Absolute, out uri);
            var data = await GetData();

            using (var httpclient = new HttpClient())
            {
                using (var content = new FormUrlEncodedContent(data))
                {
                    using (var responseMessages = await httpclient.PostAsync(uri, content))
                    {
                        string x = await responseMessages.Content.ReadAsStringAsync();
                        if (x == "OK")
                        {
                            Logger.ClearAll();
                            Logger.ClearLastFm();
                        }

                    }
                }
            }
        }
        
        private async Task<List<KeyValuePair<string, string>>> GetData()
        {
            var data = new List<KeyValuePair<string, string>>();
            string logFG = await Logger.Read();
            string logBG = await Logger.ReadBG();
            string logLastFm = await Logger.ReadLastFm();
            try
            {
                Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation deviceInfo = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
                data.Add(new KeyValuePair<string,string>("FriendlyName",deviceInfo.FriendlyName));
                data.Add(new KeyValuePair<string,string>("OperatingSystem",deviceInfo.OperatingSystem));
                data.Add(new KeyValuePair<string,string>("SystemFirmwareVersion",deviceInfo.SystemFirmwareVersion));
                data.Add(new KeyValuePair<string,string>("SystemHardwareVersion",deviceInfo.SystemHardwareVersion));
                data.Add(new KeyValuePair<string,string>("SystemManufacturer",deviceInfo.SystemManufacturer));
                data.Add(new KeyValuePair<string,string>("SystemProductName",deviceInfo.SystemProductName));
            }
            catch (Exception ex)
            {
                data.Add(new KeyValuePair<string,string>("FriendlyName","---"));
                data.Add(new KeyValuePair<string,string>("OperatingSystem","---"));
                data.Add(new KeyValuePair<string,string>("SystemFirmwareVersion","---"));
                data.Add(new KeyValuePair<string,string>("SystemHardwareVersion","---"));
                data.Add(new KeyValuePair<string,string>("SystemManufacturer","---"));
                data.Add(new KeyValuePair<string,string>("SystemProductName","---"));
            }
            data.Add(new KeyValuePair<string,string>("logFG",logFG));
            data.Add(new KeyValuePair<string,string>("logBG",logBG));
            data.Add(new KeyValuePair<string, string>("logLastFm", logLastFm));
            data.Add(new KeyValuePair<string,string>("Date",DateTime.Now.ToString()));
            return data;
        }
    }
}
