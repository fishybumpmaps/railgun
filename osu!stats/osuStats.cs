using Core;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using System.Linq;
using System;

namespace osu
{
    static class osuStats
    {
        private static string apiKey = "";
        private static string apiUrl = "osu.ppy.sh/api/";
        private static List<UserCache> userCache = new List<UserCache>();

        // Set the api key
        public static void SetApiKey(string key)
        {
            apiKey = key;
        }

        // Make an api request
        public static string ApiRequest(string action, Dictionary<string, string> parameters)
        {
            string url = apiUrl + action + "?k=" + apiKey;
            string cache = "[]";

            // Append params to url
            foreach (KeyValuePair<string, string> param in parameters)
            {
                url += "&" + param.Key + "=" + param.Value;
            }

            WebClient webClient = new WebClient();

            webClient.Headers["User-Agent"] = "Railgun osu!stats extension";

            try
            {
                Log.Write(LogLevels.INFO, "osu!stats", "Attempting to do " + action + "...");
                cache = webClient.DownloadString("https://" + url);
            } catch
            {
                try
                {
                    Log.Write(LogLevels.WARNING, "osu!stats", "Falling back to insecure HTTP...");
                    cache = webClient.DownloadString("http://" + url);
                } catch
                {
                    Log.Write(LogLevels.ERROR, "osu!stats", "Failed to connect to the osu!api!");
                }
            }

            return cache;
        }

        public static GetUser GetUser(string user)
        {
            // Check if there's a cache entry
            UserCache cached = null;
            try {
                cached = userCache.Single(cache => cache.username == user);
            } catch { }

            if (cached != null)
            {
                TimeSpan cacheAge = DateTime.Now - cached.date;
                if (cacheAge.TotalMinutes <= 5)
                {
                    Log.Write(LogLevels.INFO, "osu!stats", "Getting " + user + " from cache.");
                    return cached.data;
                } else
                {
                    userCache.Remove(cached);
                }
            }

            string json = ApiRequest("get_user", new Dictionary<string, string>() { { "u", user } });

            GetUser get;

            // Check the length of the return to see if the user doesn't exist
            if (json.Length < 3)
            {
                get = new GetUser();
            }
            else
            {
                get = JsonConvert.DeserializeObject<List<GetUser>>(json)[0];
            }

            // Add to cache
            Log.Write(LogLevels.INFO, "osu!stats", "Added " + user + " to cache.");
            userCache.Add(new UserCache() {
                date = DateTime.Now,
                username = user,
                data = get
            });

            return get;
        }
    }
}
