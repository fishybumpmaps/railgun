using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WebSocketSharp;

namespace Railgun.Protocols.SockLegacy
{
    public class Protocol : IProtocol
    {
        // Protocol container (documentation: https://flashii.net/forum/thread/122)
        private WebSocket Socket;

        // User ID of the currently authenticated user (used internally for server protocol messaging)
        private int UserId = 0;

        // Ping interval timer
        private Timer PingInterval;

        // Server protocol message separator
        private char Separator = '\t';

        // Server user
        private User ServerUser;

        // Current interval channel (legacy sock chat can only be in one channel at once)
        private string Channel;

        public Protocol()
        {
            // Create the bot user
            ServerUser = new User(
                -1,
                "Server",
                "inherit"
            );

            // Fetch server address from the config
            string server = Config.Read("Meta", "server");

            if (server == null || server.Length < 1)
            {
                Config.Write("Meta", "server", "ws://enter_server_address_here");
            }

            // Create the websocket object
            Socket = new WebSocket(server);

            // Assign the event listeners
            Socket.OnOpen += OnOpen;
            Socket.OnClose += OnClose;
            Socket.OnError += OnError;
            Socket.OnMessage += OnMessage;

            // Connect
            Socket.Connect();
        }

        ~Protocol()
        {
            Socket.Close();
            Console.WriteLine("destructing");
        }

        private void OnOpen(object sender, EventArgs ev)
        {
            // Send authentication data
            Socket.Send(Pack(
                Client.AUTH,
                Config.Section("Auth").Values.ToArray()
            ));
        }

        private void OnClose(object sender, CloseEventArgs ev)
        {
            StopKeepAlive();

            Log.Write(LogLevels.INFO, "SockLegacy", string.Format("The connection closed with exit code {0}", ev.Code));
        }

        private void OnError(object sender, ErrorEventArgs ev)
        {
            Log.Write(LogLevels.ERROR, "SockLegacy", ev.Message);
        }

        private void OnMessage(object sender, MessageEventArgs ev)
        {
            string[] data = Unpack(ev.Data);
            Server method = (Server)int.Parse(data[0]);

            switch (method)
            {
                case Server.JOIN:
                    if (UserId == 0)
                    {
                        if (data[1] == "y")
                        {
                            UserId = int.Parse(data[2]);

                            if (Channel == null)
                            {
                                Channel = data[6];
                            }

                            StartKeepAlive();
                        } else
                        {
                            switch (data[2])
                            {
                                case "joinfail":
                                    if (data[2] == "-1")
                                    {
                                        Log.Write(LogLevels.ERROR, "SockLegacy", "You are permanently banned.");
                                    }
                                    else
                                    {
                                        Log.Write(LogLevels.ERROR, "SockLegacy", string.Format("You are banned until {0}.", EpochFrom(long.Parse(data[3])).ToLongDateString()));
                                    }
                                    break;

                                case "authfail":
                                    Log.Write(LogLevels.ERROR, "SockLegacy", "Authentication failed.");
                                    break;

                                default:
                                    Log.Write(LogLevels.ERROR, "SockLegacy", "Connection failed.");
                                    break;
                            }

                            Socket.Close();
                            break;
                        }
                    }

                    User joinUser = new User(int.Parse(data[2]), data[3], data[4], UserId == int.Parse(data[2]));
                    joinUser.SetPermissions(data[5]);

                    if (joinUser.ActiveUser)
                    {
                        break;
                    }

                    Log.Write(LogLevels.INFO, "SockLegacy", joinUser.Username + " joined.");

                    Users.Add(joinUser);
                    break;

                case Server.MESSAGE:
                    Console.WriteLine(data[2] + ": " + data[3]);
                    break;

                case Server.LEAVE:
                    User leaveUser = Users.Get(int.Parse(data[1]));

                    Log.Write(LogLevels.INFO, "SockLegacy", leaveUser.Username + " left.");

                    Users.Remove(leaveUser);
                    break;

                case Server.CHANNEL:
                    Log.Write(LogLevels.INFO, "SockLegacy", "Channel update.");
                    break;

                case Server.MOVE:
                    Log.Write(LogLevels.INFO, "SockLegacy", "User moved channels.");
                    break;

                case Server.DELETE:
                    Log.Write(LogLevels.INFO, "SockLegacy", "Message was deleted.");
                    break;

                case Server.POPULATE:
                    int populateMode = int.Parse(data[1]);
                    
                    switch (populateMode)
                    {
                        case 0:
                            int amount = int.Parse(data[2]);
                            int start = 3;

                            for (int i = 0; i < amount; i++)
                            {
                                User populateUser = new User(
                                    int.Parse(data[start]),
                                    data[start + 1],
                                    data[start + 2]
                                );

                                populateUser.SetPermissions(data[start + 3]);

                                Log.Write(LogLevels.INFO, "SockLegacy", populateUser.Username + " was already present.");

                                Users.Add(populateUser);

                                start = start + 5;
                            }
                            break;

                        case 1:
                            // msgs
                            break;

                        case 2:
                            // chnls
                            break;
                    }
                    break;

                case Server.CLEAR:
                    User clearKeep = Users.Get(UserId);

                    switch (int.Parse(data[1]))
                    {
                        case 0:
                            // remove all messages
                            break;

                        case 1:
                            Users.Clear();
                            Users.Add(clearKeep);
                            break;

                        case 2:
                            // remove all channels
                            break;

                        case 3:
                            // messages delete
                            Users.Clear();
                            Users.Add(clearKeep);
                            break;

                        case 4:
                            // messages delete
                            Users.Clear();
                            Users.Add(clearKeep);
                            // channels delete
                            break;
                    }
                    break;

                case Server.BAN:
                    bool banReconnectPossible = true;

                    if ((data[1] == "ban" || data[1] == "1"))
                    {
                        banReconnectPossible = false;

                        if (data[2] == "-1")
                        {
                            Log.Write(LogLevels.ERROR, "SockLegacy", "You are permanently banned.");
                        } else
                        {
                            Log.Write(LogLevels.ERROR, "SockLegacy", string.Format("You are banned until {0}.", EpochFrom(long.Parse(data[2])).ToLongDateString()));
                        }
                    }

                    if (banReconnectPossible)
                    {
                        Log.Write(LogLevels.WARNING, "SockLegacy", "Connection was forcefully closed (kicked), attempting reconnect!");
                        UserId = 0;
                        Socket.Connect();
                    }
                    break;

                case Server.USER:
                    User updateUser = Users.Get(int.Parse(data[1]));
                    string oldUsername = updateUser.Username;

                    updateUser.id = int.Parse(data[1]);
                    updateUser.Username = data[2];
                    updateUser.Colour = data[3];

                    updateUser.SetPermissions(data[4]);

                    Log.Write(LogLevels.INFO, "SockLegacy", oldUsername + " is now " + updateUser.Username);

                    Users.Update(updateUser.id, updateUser);
                    break;
            }
        }

        private string Pack(string[] data)
        {
            return string.Join(Separator.ToString(), data);
        }

        private string Pack(Client command, string[] data)
        {
            return command.GetHashCode().ToString() + Separator.ToString() + Pack(data);
        }

        private string[] Unpack(string data)
        {
            return data.Split(Separator);
        }
        private static DateTime EpochFrom(long unix)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unix);
        }

        private void StartKeepAlive()
        {
            PingInterval = new Timer(KeepAlive, null, 0, 30000);
        }

        private void StopKeepAlive()
        {
            PingInterval.Dispose();
            PingInterval = null;
        }

        private void KeepAlive(object state = null)
        {
            Socket.Send(Pack(
                Client.PING,
                new string[] { UserId.ToString() }
            ));
        }

        public void SendMessage(string text)
        {
            Socket.Send(Pack(
                Client.MESSAGE,
                new string[] { UserId.ToString(), text }
            ));
        }

        public int State()
        {
            return (int)Socket.ReadyState;
        }
    }
}
