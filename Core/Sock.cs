using System;
using System.Threading;
using WebSocketSharp;

namespace Core
{
    public class Sock
    {
        public bool connected = false; // Are we connected?
        private WebSocket ws; // WebSocket container
        private string server; // Server address
        private string[] authData; // Authentication data
        private Timer keepAlive; // Keep-alive ping
        private string keepAliveStr = ""; // Keep-alive attribute
        private int connectFails = 0; // Amount of times the connection failed to restart

        // Initialisation
        public Sock(string server, string[] authData)
        {
            // Set variables
            this.server = server;
            this.authData = authData;

            // Create the websocket object
            ws = new WebSocket(@"ws://" + server);

            // Register event handlers
            ws.OnOpen += ConnectionOpened;
            ws.OnError += ConnectionError;
            ws.OnMessage += ConnectionReceiveMsg;

            // Connect!
            OpenConnection();
        }

        private void ConnectionReceiveMsg(object sender, MessageEventArgs e)
        {
            switch(e.Type)
            {
                case Opcode.Text:
                    // Unpack the returned message
                    string[] data = Message.Unpack(e.Data);
#if DEBUG
                    Log.Write(0, "RawSock", e.Data);
#endif
                    // Check if we're already connected
                    if (connected)
                    {
                        new Thread(delegate ()
                        {
                            Chat.HandleMessage(data);
                        }).Start();
                    } else
                    {
                        // Authentication failed
                        if (data[0] == "1" && data[1] == "n")
                        {
                            connected = false;
                            Log.Write(2, "Core", "Authentication failed. Check your authentication data in Config.ini!");
                        } else if(data[0] == "1" && data[1] == "y")
                        {
                            connected = true;
                            Chat.userId = int.Parse(data[2]);
                            Chat.userName = data[3];
                            keepAliveStr = data[2];
                            Users.Add(int.Parse(data[2]), data[3], data[4], data[5]);
                            Log.Write(0, "Core", "Connected!");
                        }
                    }
                    break;
                default:
                    Log.Write(1, "Core", "Received non-text data, ignoring.");
                    break;
            }
        }

        // Handle connection errors
        private void ConnectionError(object sender, ErrorEventArgs e)
        {
            connectFails += 1;

            if (connectFails <= 5) {
                Log.Write(1, "Core", "An error occurred in the connection, will attempt to recover.");
                CloseConnection();
                OpenConnection();
            } else {
                Log.Write(2, "Core", "Failed to restart the connection 5 times, terminating the bot.");
                Core.Shutdown();
            }
        }

        // Send authentication data on load
        private void ConnectionOpened(object sender, EventArgs e)
        {
            // Send message 1, this is auth
            ws.Send(Message.Pack(1, Message.PackArray(authData)));
        }

        // Send data
        public void ConnectionSend(string data)
        {
            try {
                ws.Send(data);
            } catch {
                Log.Write(2, "Core", "Sending data failed?");
                ConnectionError(null, null);
            }

        }

        // Close the connection
        public void CloseConnection()
        {
            connected = false;
            ws.Close();
        }

        // Open the connection
        public void OpenConnection()
        {
            ws.Connect();

            // Start the keep-alive timer
            keepAlive = new Timer(Ping, null, 0, 60 * 1000);
        }

        // Ping
        public void Ping(object state)
        {
            ConnectionSend(Message.Pack(0, keepAliveStr));
        }
    }
}
