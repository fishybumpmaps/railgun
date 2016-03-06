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
        private static Dictionary<int, DateTime> floodLimit = new Dictionary<int, DateTime>(); // Per user flood limit
        public static int messageLimit = 10;
        public static int lastSender = 0;
        public static User lastUser;

        // Send messages
        public static void SendMessage(string text)
        {
            Core.sock.ConnectionSend(Message.Pack(2, Message.PackArray(new string[] { userId.ToString(), text })));
        }

        // Poke the flood limit
        public static void UpdateFlood(int userId)
        {
            User user = Users.Get(userId);

            if (floodLimit.ContainsKey(user.id))
            {
                floodLimit[user.id] = DateTime.Now;
            } else {
                floodLimit.Add(user.id, DateTime.Now);
            }
        }

        // Check the flood limit
        public static bool CheckFlood(int userId)
        {
            // Check if the key exists
            if (!floodLimit.ContainsKey(userId)) {
                return false;
            }

            // Get the value for the current user
            TimeSpan currentUserFlood = DateTime.Now - floodLimit[userId];

            // Check if it has been 5 seconds since the last action
            if (currentUserFlood.TotalSeconds < messageLimit)
            {
                return true;
            }

            return false;
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
                    lastUser = Users.Add(int.Parse(data[2]), data[3], data[4], data[5]);
                    Log.Write(0, "Core", data[3] + " has joined.");
                    break;

                // Chat messages
                case 2:
                    // Get the sending user
                    user = Users.Get(int.Parse(data[2]));

                    lastUser = user;

                    // Update lastSender
                    lastSender = user.id;

                    // Strip bbcodes
                    string message = Regex.Replace(data[3], @"\[[^]]+\]", "");

                    // Internal commands
                    if (message.StartsWith("!"))
                    {
                        // Check flood limit
                        if (CheckFlood(user.id))
                        {
                            break;
                        }

                        // Update flood limit
                        UpdateFlood(user.id);

                        string[] cArgs = message.Substring(1).Split(' ');

                        switch(cArgs[0])
                        {
                            case "extensions:reload":
                                Log.Write(0, "Core", "Extension reload was issued by " + user.userName);
                                SendMessage("Reloading all extensions...");
                                Core.LoadExtensions();
                                SendMessage("Finished!");
                                break;

                            case "extensions:loaded":
                                string extensions = "Loaded extensions:";
                                foreach (IExtension extension in Core.Extensions)
                                {
                                    extensions += "\r\n" + extension.Name;
                                }
                                SendMessage(extensions);
                                break;

                            case "core:uptime":
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
                            case "debug:floodlimits":
                                foreach (KeyValuePair<int, DateTime> limit in floodLimit)
                                {
                                    SendMessage(limit.Key.ToString() + ":" + limit.Value.ToString());
                                }
                                break;
                                
                            case "debug:quit":
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

                    lastUser = user;

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
                    if(int.Parse(data[1]) < 1)
                    {
                        // Joined the channel
                        lastUser = Users.Add(int.Parse(data[2]), data[3], data[4], data[5]);
                        Log.Write(0, "Core", data[3] + " has joined the channel.");
                    } else
                    {
                        // Left the channel
                        user = Users.Remove(int.Parse(data[2]));
                        lastUser = user;
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
                            // We don't have to do anything with this.
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
                    lastUser = Users.Update(int.Parse(data[1]), data[2], data[3], data[4]);
                    Log.Write(0, "Core", "A user was updated.");
                    break;
            }

            /*foreach(IExtension extension in Core.Extensions)
            {
                new Thread(delegate () {
                    extension.Handle(data);
                }).Start();
            }*/
        }
    }
}
