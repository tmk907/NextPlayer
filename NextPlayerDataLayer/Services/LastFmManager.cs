﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using NextPlayerDataLayer.Helpers;
using NextPlayerDataLayer.Constants;
using NextPlayerDataLayer.Diagnostics;

namespace NextPlayerDataLayer.Services
{
    public enum ErrorCode
    {
        ReAuth,
        Cache,
        Nothing
    }

    public class TrackScrobble
    {
        public string Artist { get; set; }
        public string Track { get; set; }
        public string Timestamp { get; set; }
    }

    public sealed class LastFmManager
    {
        private const string ApiKey = "";
        private const string ApiSecret = "";

        private const string RootUrl = "http://ws.audioscrobbler.com/2.0/";
        private const string RootAuth = "https://ws.audioscrobbler.com/2.0/";

        private string Username = "";
        private string Password = "";

        private string SessionKey = "";

        private static readonly LastFmManager current = new LastFmManager();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static LastFmManager()
        {
        }

        public static LastFmManager Current
        {
            get
            {
                return current;
            }
        }
        
        private LastFmManager()
        {
            //Username = "tmk907";
            //Password = "tom108pl";
            Username = (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmLogin) ?? String.Empty).ToString();
            Password = (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmPassword) ?? String.Empty).ToString();
            SessionKey = (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmSessionKey) ?? String.Empty).ToString();
            if (AreCredentialsSet())
            {
                SetSessionAndSendCached();
            }
        }

        private bool IsSessionOn()
        {
            return SessionKey != "";
        }

        private bool AreCredentialsSet()
        {
            return Username != "" && Password != "";
        }

        private bool IsStatusOK(string response)
        {
            return response.Contains("<lfm status=\"ok\">");
        }

        private string GetSignature(Dictionary<string,string> parameters)
        {
            StringBuilder builder = new StringBuilder();
            var ordered = parameters.OrderBy(p=>p.Key, StringComparer.Ordinal);

            foreach(var item in ordered)
            {
                builder.Append(item.Key).Append(item.Value);
            }
            builder.Append(ApiSecret);

            HashAlgorithmProvider md5hasher = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var buffer = CryptographicBuffer.ConvertStringToBinary(builder.ToString(), BinaryStringEncoding.Utf8);
            IBuffer buffHash = md5hasher.HashData(buffer);

            return CryptographicBuffer.EncodeToHexString(buffHash);
        }

        private async Task<string> SendMessage(Dictionary<string,string> data, bool isHttps)
        {
            string response = "";
            string host;
            if (isHttps)
            {
                host = RootAuth;
            }
            else
            {
                host = RootUrl;
            }
            using(HttpClient httpClient = new HttpClient())
            {
                using(var content = new FormUrlEncodedContent(data))
                {
                    using (var result = await httpClient.PostAsync(host, content))
                    {
                        response = await result.Content.ReadAsStringAsync();
                    }
                }
            }
            return response;
        }

        public async Task SetMobileSession()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("username", Username);
            data.Add("password", Password);
            data.Add("method", "auth.getMobileSession");
            data.Add("api_key",ApiKey);
            string signature = GetSignature(data);
            data.Add("api_sig", signature);

            string response = await SendMessage(data, true);

            if (response.Contains("<lfm status=\"ok\">"))
            {
                int i1 = response.IndexOf("<key>") + "<key>".Length;
                int i2 = response.IndexOf("</key>");
                SessionKey = response.Substring(i1,i2-i1);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmSessionKey, SessionKey);
            }
            else
            {
                ParseError(response);
            }
        }

        private void PrepareAuth(ref Dictionary<string, string> msg)
        {
            msg.Add("api_key", ApiKey);
            msg.Add("sk", SessionKey);
            string signature = GetSignature(msg);
            msg.Add("api_sig", signature);
        }

        private ErrorCode ParseError(string response)
        {
            ErrorCode he = ErrorCode.Nothing;

            if (response.Contains("<error code="))
            {
                Logger.SaveLastFm(response);
                int i1 = response.IndexOf("code=\"") + "code=\"".Length;
                int i2 = response.IndexOf("\"", i1);
                int code;
                if (Int32.TryParse(response.Substring(i1, i2 - i1), out code))
                {
                    switch (code)
                    {
                        case 9:
                            he = ErrorCode.ReAuth;
                            break;
                        case 11:
                            he = ErrorCode.Cache;
                            break;
                        case 16:
                            he = ErrorCode.Cache;
                            break;
                    }
                }
            }
            return he;
        }

        private async Task HadleError(ErrorCode code, string function, object data)
        {
            if (code == ErrorCode.ReAuth)
            {
                SessionKey = "";
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmSessionKey, "");
                await SetMobileSession();
            }
            if (code != ErrorCode.Nothing)
            {
                string info = "";
                switch (function)
                {
                    case "track.scrobble":
                        List<TrackScrobble> list = (List<TrackScrobble>)data;
                        foreach(var item in list)
                        {
                            await DatabaseManager.SaveAsync(function, item.Artist, item.Track, item.Timestamp);
                        }
                        info = list[0].Artist + " " + list[0].Track;
                        break;
                    case "track.love":
                        Tuple<string, string> t = (Tuple<string, string>)data;
                        await DatabaseManager.SaveAsync(function, t.Item1, t.Item2);
                        info = t.Item1 + "" + t.Item2;
                        break;
                    case "track.unlove":
                        Tuple<string, string> t2 = (Tuple<string, string>)data;
                        await DatabaseManager.SaveAsync(function, t2.Item1, t2.Item2);
                        info = t2.Item1 + "" + t2.Item2;
                        break;
                    case "track.nowplaying":
                        info = "nowplaying";
                        break;
                }
                Logger.SaveLastFm("Error Last.fm " + function + Environment.NewLine + info);
            }
        }

        private async Task TrackScroblle(List<TrackScrobble> data)
        {
            if (!AreCredentialsSet()) return;
            Dictionary<string, string> msg = new Dictionary<string, string>();
            if (data.Count == 1)
            {
                msg.Add("artist", data[0].Artist);
                msg.Add("track", data[0].Track);
                msg.Add("timestamp", data[0].Timestamp);
            }
            else
            {
                for(int i = 0; i < data.Count; i++)
                {
                    msg.Add("artist[" + i + "]", data[i].Artist);
                    msg.Add("track[" + i + "]", data[i].Track);
                    msg.Add("timestamp[" + i + "]", data[i].Timestamp);
                }
            }
            
            msg.Add("method", "track.scrobble");
            PrepareAuth(ref msg);

            string response = await SendMessage(msg, false);
            if (!IsStatusOK(response))
            {
                await HadleError(ParseError(response), "track.scrobble", data);
            }
        }

        private async Task TrackLove(string artist, string track)
        {
            if (!AreCredentialsSet()) return;
            Dictionary<string, string> msg = new Dictionary<string, string>
            {
                {"artist", artist},
                {"track", track},
                {"method", "track.love"}
            };
            PrepareAuth(ref msg);

            string response = await SendMessage(msg, false);
            if (!IsStatusOK(response))
            {
                var er = ParseError(response);
                await HadleError(er, "track.love", new Tuple<string,string>(artist,track));
            }
        }

        private async Task TrackUnlove(string artist, string track)
        {
            if (!AreCredentialsSet()) return;
            Dictionary<string, string> msg = new Dictionary<string, string>
            {
                {"artist", artist},
                {"track", track},
                {"method", "track.unlove"}
            };
            PrepareAuth(ref msg);

            string response = await SendMessage(msg, false);
            if (!IsStatusOK(response))
            {
                var er = ParseError(response);
                await HadleError(er, "track.unlove", new Tuple<string, string>(artist, track));
            }
        }

        public async Task TrackUpdateNowPlaying(string artist, string track)
        {
            if (!AreCredentialsSet()) return;
            Dictionary<string, string> msg = new Dictionary<string, string>
            {
                {"artist", artist},
                {"track", track},
                {"method", "track.updateNowPlaying"}
            };
            PrepareAuth(ref msg);

            string response = await SendMessage(msg, false);
            if (!IsStatusOK(response))
            {
                await HadleError(ParseError(response), "track.updateNowPlaying", null);
            }
        }

        public async Task<bool> Login(string login, string password)
        {
            Username = login;
            Password = password;
            await SetMobileSession();
            return IsSessionOn();
        }

        public void Logout()
        {
            Username = "";
            Password = "";
            SessionKey = "";
        }
        
        public async Task SendCachedScrobbles()
        {
            if (!AreCredentialsSet()) return;
            var savedScrobbles = DatabaseManager.ReadAndDeleteAll();
            List<TrackScrobble> tracks = new List<TrackScrobble>();
            foreach(var scrobble in savedScrobbles)
            {
                switch (scrobble["function"])
                {
                    case "track.scrobble":
                        tracks.Add(new TrackScrobble() { Artist = scrobble["artist"], Timestamp = scrobble["timestamp"], Track = scrobble["track"] });
                        break;
                    case "track.love":
                        await TrackLove(scrobble["artist"], scrobble["track"]);
                        break;
                    case "track.unlove":
                        await TrackUnlove(scrobble["artist"], scrobble["track"]);
                        break;
                }
            }
            while (tracks.Count > 50)
            {
                await TrackScroblle(tracks.Take(50).ToList());
                tracks.RemoveRange(0, 50);
            }
            if (tracks.Count > 0)
            {
                await TrackScroblle(tracks);
            }
        }

        private async Task SetSessionAndSendCached()
        {
            if (!IsSessionOn())
            {
                await SetMobileSession();
            }
            //await SendCachedScrobbles();
        }

        public void CacheTrackScrobble(TrackScrobble scrobble)
        {
            DatabaseManager.Save("track.scrobble", scrobble.Artist, scrobble.Track, scrobble.Timestamp);
        }

        public async Task CacheTrackLove(string artist, string track)
        {
            await DatabaseManager.UpdateFavoriteAsync("track.love", artist, track);
        }

        public async Task CacheTrackUnlove(string artist, string track)
        {
            await DatabaseManager.UpdateFavoriteAsync("track.unlove", artist, track);
        }
    }
}
