using Core;
using Extensions;
using System.Net;
using Newtonsoft.Json;
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
            Log.Write(0, "LastFM", "Initialised Last.FM extension.");
        }

        public void Destruct()
        {
        }

        public void Handle(string[] data)
        {
            // Ignore if this isn't a normal chat message or if this isn't a !np message
            if (data[0] != "2" || !Regex.Replace(data[3], @"\[[^]]+\]", "").StartsWith("!np"))
            {
                return;
            }

            string message = Regex.Replace(data[3], @"\[[^]]+\]", ""),
                    flashii = "",
                    lastfm = "";
            string[] msgParts = message.Split(' ');

            // Check if a second part if set and it isn't blank
            if(msgParts.Length > 1)
            {
                flashii = msgParts[1];
            } else
            {
                flashii = data[2];
            }

            // Create a WebClient object
            WebClient webClient = new WebClient();

            // Set headers
            webClient.Headers["User-Agent"] = "Shinoa Last.FM Extension";
            
            // Try to get the last.fm username
            try
            {
                lastfm = webClient.DownloadString("http://flashii.net/spookyshit/lastfm.php?u=" + flashii + "&z=meow");
            } catch
            {
                return;
            }

            Chat.SendMessage(lastfm);

            /*try
            {
                Log.Write(0, "LastFM", "Getting Last.FM data for " + user.userName);

                string result = request.DownloadString(@"http://flashii.net/koishi/interface.php?m=lfm&u=" + data[2]);
                // Attempt to parse the result
                NowPlaying = JsonConvert.DeserializeObject<NowPlaying>(result);
            }
            catch
            {
                Log.Write(0, "LastFM", "Failed to get Last.FM data for " + user.userName);
                NowPlaying = new NowPlaying() { IsSuccessful = false, IsListeningNow = false, Song = null };
            }

            if (NowPlaying.IsSuccessful == true)
            {
                Chat.SendMessage("[i][b][color=" + user.colour + "]" + user.userName + "[/color][/b] " + (NowPlaying.IsListeningNow ? " is listening to [b]" : " last listened to [b]") + NowPlaying.Song + "[/b][/i]");
            } else {
                Chat.SendMessage("[i][b][color=" + user.colour + "]" + user.userName + "[/color][/b] doesn't have a last.fm account hooked to their account![/i]");
            }*/
        }
    }
}
