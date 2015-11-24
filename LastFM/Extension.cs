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

            string message = Regex.Replace(data[3], @"\[[^]]+\]", "");

            // Create a WebClient object
            WebClient request = new WebClient();

            // Set headers
            request.Headers["User-Agent"] = "Satori Last.FM Extension";

            // Define
            NowPlaying NowPlaying;
            User user;

            // Attempt to get the user
            try
            {
                user = Users.Get(int.Parse(data[2]));
            } catch {
                return;
            }

            try
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
            }
        }
    }
}
