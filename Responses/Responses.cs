using Newtonsoft.Json;
using Core;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using System;

namespace Responses
{
    public static class Responses
    {
        public static List<Trigger> Triggers;

        public static void Load()
        {
            if(File.Exists("Data\\Responses.json"))
            {
                using (StreamReader read = new StreamReader("Data\\Responses.json"))
                {
                    string json = read.ReadToEnd();
                    Triggers = JsonConvert.DeserializeObject<List<Trigger>>(json);
                }
                Log.Write(0, "Responses", "Loaded responses.");
            } else
            {
                Log.Write(1, "Responses", "Responses.json does not exist!");
            }
        }

        public static void Handle(string[] data)
        {
            // Only handle text message
            if(data[0] != "2")
            {
                return;
            }

            // Define message
            string message;

            if (data[2] == "-1")
            {
                message = data[3].Split('\f')[2];
            } else {
                message = Regex.Replace(data[3], @"\[[^]]+\]", "");
            }

            // Reload
            if(message == "!responses:reload")
            {
                Log.Write(0, "Responses", "Reload requested in chat.");
                Load();
                Chat.SendMessage("[i]Responses: Reloaded responses file.[/i]");
            }

            Trigger trigger = null;

            try
            {
                trigger = Triggers.Single(find => find.Triggers.Contains(message));
            } catch { }

            if(trigger != null)
            {
                foreach(string responseRaw in trigger.Responses[(new Random()).Next(trigger.Responses.Count)])
                {
                    // Get user details
                    User user = Users.Get(int.Parse(data[2]));

                    string response = responseRaw
                        .Replace("@UNAME", user.userName)
                        .Replace("@UCOLOUR", user.colour)
                        .Replace("@UID", user.id.ToString());

                    // Do a 1 second sleep for a "realistic" effect
                    Thread.Sleep(900);

                    Chat.SendMessage(response);
                }
            }
        }
    }
}
