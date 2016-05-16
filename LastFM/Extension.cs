using Core;
using Extensions;
using System.Net;
using System.Xml;
using System.Text.RegularExpressions;

namespace LastFM
{
    public class Extension : IExtension
    {
        public string Name
        {
            get
            {
                return "LastFM";
            }
        }

        public void Initialise()
        {
            Log.Write(LogLevels.INFO, "LastFM", "Loading Last.FM settings.");
            string osuApiKey = Config.Read("LastFM", "apiKey");

            if (osuApiKey.Length < 1)
            {
                Config.Write("LastFM", "apiKey", "api_key_here");
                Log.Write(LogLevels.ERROR, "LastFM", "No API key was set in the settings file, a placeholder value has been created!");
            }
            else
            {
                LastFM.SetApiKey(osuApiKey);
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

                // !np
                if (message.StartsWith("!np"))
                {
                    string[] cArgs = message.Substring(1).Split(' ');

                    // Create a WebClient object
                    WebClient webClient = new WebClient();

                    // Set headers
                    webClient.Headers["User-Agent"] = "Railgun Last.FM Extension";

                    string[] usernames = { null, null };
                    string username = "";

                    try
                    {
                        username = cArgs[1];
                    }
                    catch
                    {
                        username = data[2];
                    }


                    // Try to get the last.fm username
                    try
                    {
                        usernames = webClient.DownloadString("http://flashii.net/web/lastfm.php?u=" + username + "&z=meow").Split('|');
                    }
                    catch
                    {
                        Log.Write(LogLevels.ERROR, "LastFM", "Failed to connect to Flashii!");
                        return;
                    }

                    // Check if usernames isn't empty
                    if (usernames.Length < 2)
                    {
                        Chat.SendMessage("[i][b]" + username + "[/b] is not a member here or does not have a Last.FM set to their account![/i]");
                        return;
                    }

                    XmlDocument apiReturn = null;

                    try
                    {
                        apiReturn = LastFM.LatestTrack(usernames[1]);
                    }
                    catch {
                        Log.Write(LogLevels.WARNING, "LastFM", "Something went wrong while getting the API return.");
                        return;
                    }

                    if (apiReturn.GetElementsByTagName("name").Count < 2)
                    {
                        Chat.SendMessage("[i][b]" + username + "[/b] does not have an active Last.FM account set to their account![/i]");
                        return;
                    }

                    string send = "[i][b]" + usernames[0] + "[/b] ",
                           track = apiReturn.GetElementsByTagName("name")[1].InnerText,
                           trackUrl = apiReturn.GetElementsByTagName("url")[1].InnerText,
                           artist = apiReturn.GetElementsByTagName("name")[0].InnerText,
                           aristUrl = apiReturn.GetElementsByTagName("url")[0].InnerText;
                    bool nowListening = apiReturn.GetElementsByTagName("track")[0].Attributes["nowplaying"] != null;

                    send += (nowListening ? "is listening" : "last listened") + " to ";
                    send += "[url=" + trackUrl + "]" + track + "[/url]";
                    send += (artist.Length > 1 && track.Length > 1 ? " by " : "");
                    send += "[url=" + aristUrl + "]" + artist + "[/url]";
                    send += "[/i]";

                    Chat.SendMessage(send);
                }
            }
        }
    }
}
