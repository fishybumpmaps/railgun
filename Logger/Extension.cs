using Railgun;
using Extensions;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Logger
{
    public class Extension : IExtensionV1
    {
        public static bool active = false;
        private static StreamWriter logWriter;

        public string Name
        {
            get
            {
                return "Logger";
            }
        }

        public void Initialise()
        {
            Log.Write(LogLevels.INFO, "Logger", "Initialised Chat Logger.");
            
            // Create log filename
            string filename = "chat_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log";
            // Check if the destination directory exists
            if (!Directory.Exists(Railgun.Constants.Directories.DATA + "/ChatLogs/"))
            {
                Directory.CreateDirectory(Railgun.Constants.Directories.DATA + "/ChatLogs/");
            }
            // Create stream writer
            logWriter = new StreamWriter(Railgun.Constants.Directories.DATA + "/ChatLogs/" + filename);
            // Enable auto flush
            logWriter.AutoFlush = true;

            logWriter.WriteLine("Chat Log of " + DateTime.UtcNow);
            logWriter.WriteLine("All dates and times are UTC.");
            logWriter.WriteLine("=================================================================");

            active = true;
            Log.Write(LogLevels.INFO, "Logger", "Logging chat to file " + filename + ".");
        }

        public void Destruct()
        {
            active = false;
            logWriter.Close();
            logWriter = null;
            Log.Write(LogLevels.INFO, "Logger", "Stopped logging the chat.");
        }

        public void Handle(string[] data)
        {
            if (!active)
            {
                return;
            }

            string logLine = "[" + DateTime.UtcNow + "] ";

            switch(int.Parse(data[0]))
            {
                case 1:
                    logLine += "* " + data[3] + " has joined.";
                    break;
                case 2:
                    logLine += "<" + Chat.lastUser.Username + "[ID:" + Chat.lastUser.id + "]> " + Regex.Replace(data[3], @"\[[^]]+\]", "").Replace(" <br/> ", "\n");
                    break;
                case 3:
                    logLine += "* " + Chat.lastUser.Username;
                    // Add console log
                    switch (data[3])
                    {
                        case "leave":
                            logLine += " left.";
                            break;
                        case "kick":
                            logLine += " got kicked.";
                            break;
                        default:
                            logLine += " disconnected.";
                            break;
                    }
                    break;
                case 5:
                    logLine += "* " + Chat.lastUser.Username + " " + (data[1] == "0" ? "joined" : "left") + " the channel.";
                    break;
                case 7:
                    if(int.Parse(data[1]) == 0)
                    {
                        logLine += "Currently logged in: ";

                        // Amount of users currently in the chat
                        int usersAmount = int.Parse(data[2]);
                        // Index where the users begin
                        int startIndex = 3;
                        // Iterate over the return
                        for (int i = 0; i < usersAmount; i++)
                        {
                            logLine += data[startIndex + 1] + "[ID:" + data[startIndex] + "]" + (usersAmount * 5 == startIndex + 5 ? "." : ", ");
                            startIndex = startIndex + 5;
                        }
                    } else
                    {
                        logLine = null;
                    }
                    break;
                default:
                    logLine = null;
                    break;
            }

            if (logLine != null)
            {
                logWriter.WriteLine(logLine);
            }
        }
    }
}
