using Core;
using Extensions;
using System;
using System.Text.RegularExpressions;

namespace Flap
{
    public class Extension : IExtension
    {
        public string Name
        {
            get
            {
                return "Flap";
            }
        }

        public void Initialise()
        {
            Log.Write(0, "Flap", "Flapstension loaded.");
        }

        public void Destruct()
        {
        }

        public void Handle(string[] data)
        {
            string message = "";

            try {
                message = Regex.Replace(data[3], @"\[[^]]+\]", "");
            } catch { }

            if (data[0] == "2" && message.StartsWith("!flap"))
            {
                // Create vars
                string sendMessage = "";
                Random random = new Random();
                int length = random.Next(50, 100);
                string[] args = message.Split(' ');
                string text = ":amu:";

                // Try to set text to the second arg
                try {
                    text = args[1];
                } catch { }

                // Add padding spaces to make substring not explode
                sendMessage += new string(' ', random.Next(text.Length, text.Length * 2));
                
                // Do a random thing and add amus and spaces
                for (int i = 0; i < length; i++)
                {
                    if (sendMessage.Length < 0 || sendMessage.Substring(sendMessage.Length - text.Length, text.Length) == text)
                    {
                        sendMessage += new string(' ', random.Next(0, 50));
                    } else
                    {
                        sendMessage += text;
                    }
                }

                // Send the message
                Chat.SendMessage("[code]" + (sendMessage.Length > 1988 ? sendMessage.Substring(0, 1988) : sendMessage) + "[/code]");
            }
        }
    }
}
