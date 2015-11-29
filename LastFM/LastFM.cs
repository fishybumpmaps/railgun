using Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace LastFM
{
    class LastFM
    {
        private static string apiKey = "";
        private static string apiUrl = "ws.audioscrobbler.com/2.0/";
        private static List<ApiCache> apiCache = new List<ApiCache>();

        public static void SetApiKey(string key)
        {
            apiKey = key;
        }

        public static XmlDocument ApiRequest(string action, Dictionary<string, string> parameters)
        {
            string url = apiUrl + "?method=" + action + "&api_key=" + apiKey;
            XmlDocument cache = new XmlDocument();

            foreach (KeyValuePair<string, string> param in parameters)
            {
                url += "&" + param.Key + "=" + param.Value;
            }

            // Check if there's a cache entry
            ApiCache cached = null;
            try
            {
                cached = apiCache.Single(check => check.url == url);
            }
            catch { }

            if (cached != null)
            {
                TimeSpan cacheAge = DateTime.Now - cached.date;
                if (cacheAge.TotalMinutes <= 2)
                {
                    Log.Write(0, "LastFM", "Getting " + action + " from cache.");
                    return cached.data;
                }
                else
                {
                    apiCache.Remove(cached);
                }
            }

            WebClient webClient = new WebClient();

            webClient.Headers["User-Agent"] = "Shinoa Last.FM Extension";
            webClient.Encoding = Encoding.UTF8;

            try
            {
                Log.Write(0, "LastFM", "Attempting to do " + action + "...");
                cache.LoadXml(webClient.DownloadString("https://" + url));
            }
            catch
            {
                try
                {
                    Log.Write(1, "LastFM", "Falling back to insecure HTTP...");
                    cache.LoadXml(webClient.DownloadString("http://" + url));
                }
                catch
                {
                    Log.Write(2, "LastFM", "Failed to connect to the Last.FM API!");
                }
            }

            // Add to cache
            Log.Write(0, "LastFM", "Added " + action + " to cache.");
            apiCache.Add(new ApiCache()
            {
                date = DateTime.Now,
                url = url,
                data = cache
            });

            return cache;
        }

        public static XmlDocument LatestTrack(string user)
        {
            XmlDocument doc = ApiRequest("user.getRecentTracks", new Dictionary<string, string>() { { "user", user }, { "limit", "1" }, { "extended", "1" } });
            
            return doc;
        }
    }
}
