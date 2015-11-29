using System;
using Core;
using Extensions;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;
using System.Globalization;

namespace osuStats
{
    public class Extension : IExtension
    {
        public string Name
        {
            get
            {
                return "osu!stats";
            }
        }

        public void Destruct()
        {
        }

        public void Handle(string[] data)
        {
            if (data[0] == "2")
            {
                // Clean message
                string message = Regex.Replace(data[3], @"\[[^]]+\]", "");
                if (message.StartsWith("!osu:"))
                {
                    string[] cArgs = message.Substring(5).Split(' ');

                    switch (cArgs[0])
                    {
                        case "stats":
                            // Create a WebClient object
                            WebClient webClient = new WebClient();

                            // Set headers
                            webClient.Headers["User-Agent"] = "Shinoa osu!stats Extension";

                            string[] usernames = { null, null };
                            string username = "";

                            try
                            {
                                username = cArgs[1];
                            } catch
                            {
                                username = data[2];
                            }
                            

                            // Try to get the osu username
                            try
                            {
                                usernames = webClient.DownloadString("http://flashii.net/spookyshit/osu.php?u=" + username + "&z=meow").Split('|');
                            }
                            catch
                            {
                                Log.Write(2, "osu!stats", "Failed to connect to Flashii!");
                                return;
                            }

                            // Check if usernames isn't empty
                            if(usernames.Length < 2)
                            {
                                Chat.SendMessage("[i][b]" + username + "[/b] is not a member here![/i]");
                                return;
                            }

                            GetUser osu = new GetUser();

                            // Get osu! account details
                            try
                            {
                                osu = osuStats.GetUser(usernames[1]);
                            }
                            catch { }

                            // Check if the osu username is anything
                            if (osu.user_id == null)
                            {
                                Chat.SendMessage("[i][b]" + usernames[0] + "[/b] does not have an (active) osu! account connected to their account![/i]");
                                return;
                            }

                            CultureInfo cultureInfo = new CultureInfo("en-GB");

                            Chat.SendMessage(
                                "[i][b]osu! statistics for [url=https://osu.ppy.sh/u/"
                                + (osu.user_id ?? "-1")
                                + "]"
                                + (osu.username ?? "[deleted user]")
                                + "[/url]"
                                + (usernames[0] != osu.username ? " (" + usernames[0] + ")" : "")
                                + "[/b][/i]\r\n"
                                + "Performance: "
                                + Math.Round(float.Parse((osu.pp_raw ?? "-1"), CultureInfo.InvariantCulture), MidpointRounding.AwayFromZero).ToString("N0", cultureInfo)
                                + "pp :: Rank #"
                                + long.Parse((osu.pp_rank ?? "-1")).ToString("N0", cultureInfo)
                                + " ("
                                + osu.country
                                + " #"
                                + long.Parse((osu.pp_country_rank ?? "-1")).ToString("N0", cultureInfo)
                                + ") :: Ranked Score: "
                                + long.Parse((osu.ranked_score ?? "-1")).ToString("N0", cultureInfo)
                                + " :: Accuracy: "
                                + Math.Round(float.Parse((osu.accuracy ?? "-1"), CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero).ToString("#.##", cultureInfo)
                                + "%\r\n"
                                + "Total Score: "
                                + long.Parse((osu.total_score ?? "-1")).ToString("N0", cultureInfo)
                                + " :: SS: "
                                + int.Parse((osu.count_rank_ss ?? "-1")).ToString("N0", cultureInfo)
                                + " :: S: "
                                + int.Parse((osu.count_rank_s ?? "-1")).ToString("N0", cultureInfo)
                                + " :: A: "
                                + int.Parse((osu.count_rank_a ?? "-1")).ToString("N0", cultureInfo)
                                + "\r\n"
                                + "Level: "
                                + Math.Round(float.Parse((osu.level ?? "-1"), CultureInfo.InvariantCulture))
                                + " :: Plays: "
                                + int.Parse((osu.playcount ?? "-1")).ToString("N0", cultureInfo)
                                + " :: 300s: "
                                + int.Parse((osu.count300 ?? "-1")).ToString("N0", cultureInfo)
                                + " :: 100s: "
                                + int.Parse((osu.count100 ?? "-1")).ToString("N0", cultureInfo)
                                + " :: 50s: "
                                + int.Parse((osu.count50 ?? "-1")).ToString("N0", cultureInfo)
                            );
                            break;
                    }
                }
            }
        }

        public void Initialise()
        {
            Log.Write(0, "osu!stats", "Loading osu!stats settings.");
            string osuApiKey = Utils.GetSettings().Read("OsuStats", "apiKey");

            if(osuApiKey.Length < 1)
            {
                Utils.GetSettings().Write("OsuStats", "apiKey", "api_key_here");
                Log.Write(1, "osu!stats", "No API key was set in the settings file, a placeholder value has been created!");
            } else {
                osuStats.SetApiKey(osuApiKey);
            }
        }
    }
}
