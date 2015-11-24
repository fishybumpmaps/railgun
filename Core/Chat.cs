using Extensions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace Core
{
    public class Chat
    {
        public static int userId = 0; // Chatbot user ID
        public static string userName = ""; // " " name
        private static bool shutUp = false;
        private static Dictionary<int, DateTime> floodLimit = new Dictionary<int, DateTime>(); // Per user flood limit

        // Send messages
        public static void SendMessage(string text)
        {
            if(shutUp)
            {
                return;
            }

            Core.sock.ConnectionSend(Message.Pack(2, Message.PackArray(new string[] { userId.ToString(), text })));
        }

        // Handle sock chat returns
        public static void HandleMessage(string[] data)
        {
            // Predefined user
            User user = null;

            // Switch modes
            switch (int.Parse(data[0]))
            {
                // Connection ping
                case 0:
                    break;

                // User joining
                case 1:
                    Users.Add(int.Parse(data[2]), data[3], data[4], data[5]);
                    Log.Write(0, "Core", data[3] + " has joined.");
                    break;

                // Chat messages
                case 2:
                    // Get the sending user
                    user = Users.Get(int.Parse(data[2]));

                    // Strip bbcodes
                    string message = Regex.Replace(data[3], @"\[[^]]+\]", "");

                    // Flood protection
                    if (floodLimit.ContainsKey(user.id)) {
                        // Get the value for the current user
                        DateTime currentUserFlood = floodLimit[user.id];

                        // Check if it has been 30 seconds since the last action
                        if (currentUserFlood.Second < (new DateTime()).Second + 30) {
                            return;
                        } else {
                            floodLimit[user.id] = new DateTime();
                        }
                    } else {
                        floodLimit.Add(user.id, new DateTime());
                    }

                    // Internal commands
                    if (message.StartsWith("!"))
                    {
                        string[] cArgs = message.Substring(1).Split(' ');

                        switch(cArgs[0])
                        {
                            case "extensions:reload":
                                Log.Write(0, "Core", "Extension reload was issued by " + user.userName);
                                SendMessage("Reloading all extensions...");
                                Core.LoadExtensions();
                                SendMessage("Finished!");
                                break;

                            case "uptime":
                                double uptime = Core.GetUptime();
                                TimeSpan time = TimeSpan.FromSeconds(uptime);
                                SendMessage(
                                    string.Format(
                                        userName + " has been up for {0:D2}h:{1:D2}m:{2:D2}s.",
                                        time.Hours,
                                        time.Minutes,
                                        time.Seconds
                                    )
                                );
                                break;
#if DEBUG
                            case "disintegrate":
                                Core.Shutdown(null, null);
                                break;
#endif
                        }
                    }

                    // Write message to console
                    Log.Write(0, "Core", "<" + user.userName + "> "+ data[3]);
                    break;

                // User disconnect notification
                case 3:
                    // Remove user
                    user = Users.Remove(int.Parse(data[1]));

                    // Add console log
                    switch (data[3])
                    {
                        case "leave":
                            // Left on own accord
                            Log.Write(0, "Core", user.userName + " has disconnected.");
                            break;
                        case "kick":
                            // Kick
                            Log.Write(0, "Core", user.userName + " has been kicked.");
                            break;
                        default:
                            // Somehow else
                            Log.Write(0, "Core", user.userName + " jumped out of a window and is no longer with us.");
                            break;
                    }
                    break;

                // Channel updates
                case 4:
                    Log.Write(0, "Core", "A channel update occurred.");
                    break;

                // Joining/leaving channels
                case 5:
                    if(bool.Parse(data[1]))
                    {
                        // Joined the channel
                        Users.Add(int.Parse(data[2]), data[3], data[4], data[5]);
                        Log.Write(0, "Core", data[3] + " has joined the channel.");
                    } else
                    {
                        // Left the channel
                        user = Users.Remove(int.Parse(data[2]));
                        Log.Write(0, "Core", user.userName + " has left the channel.");
                    }
                    break;

                // Message deletion
                case 6:
                    Log.Write(0, "Core", "A message was deleted.");
                    break;

                // Chat population
                case 7:
                    switch(int.Parse(data[1]))
                    {
                        // Users
                        case 0:
                            // Amount of users currently in the chat
                            int usersAmount = int.Parse(data[2]);
                            // Index where the users begin
                            int startIndex = 3;
                            // Iterate over the return
                            for (int i = 0; i < usersAmount; i++)
                            {
                                Users.Add(int.Parse(data[startIndex]), data[startIndex + 1], data[startIndex + 2], data[startIndex + 3]);
                                Log.Write(0, "Core", data[startIndex + 1] + " is already logged in.");
                                startIndex = startIndex + 5;
                            }
                            break;

                        // Messages
                        case 1:
                            // We're using this to recognise the chatbot
                            if(data[3] == "-1")
                            {
                                try
                                {
                                    Users.Get(int.Parse(data[3]));
                                } catch
                                {
                                    Users.Add(int.Parse(data[3]), data[4], data[5], "0 0 0 0 0");
                                    Log.Write(0, "Core", "Added ChatBot to userlist.");
                                }
                            }
                            break;

                        // Channels
                        case 2:
                            // Multichannel support is not needed (or even at all possible) at the current moment.
                            break;
                    }
                    break;

                // Forcefully clear a context list
                case 8:
                    Log.Write(0, "Core", "A force clear was issued.");
                    break;

                // Kick/ban
                case 9:
                    if(data[1] == "ban")
                    {
                        DateTime until = Utils.EpochFrom(long.Parse(data[2]));
                        Log.Write(0, "Core", "The bot was banned until " + until.ToString("R") + ", attempting reconnect after the ban is supposed to expire.");
                    } else
                    {
                        Log.Write(0, "Core", "The bot has been kicked, attempting to restart the connection.");
                    }
                    Core.sock.CloseConnection();
                    Core.sock = null;
                    break;

                // User information has been updated
                case 10:
                    Users.Update(int.Parse(data[1]), data[2], data[3], data[4]);
                    Log.Write(0, "Core", "A user was updated.");
                    break;
            }

            foreach(IExtension extension in Core.Extensions)
            {
                new Thread(delegate () {
                    extension.Handle(data);
                }).Start();
            }
        }
    }
}
