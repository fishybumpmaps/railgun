﻿using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Extensions;
using Protocol;

namespace Core
{
    public static class Core
    {
        public static Sock sock; // Sock interface container
        public static string[] directories = { "Logs", "Extensions", "Data", "Protocols" }; // Logs directory name
        private static ManualResetEvent shutdown = new ManualResetEvent(false); // Thing to keep the console window running
        private static DateTime startTime = DateTime.Now; // For getting the application uptime

        private static IProtocol Protocol = null;

        // Getting the uptime
        public static double GetUptime()
        {
            return (DateTime.Now - startTime).TotalSeconds;
        }
        
        // Main function
        static void Main(string[] args)
        {
            // Set console title
            Console.Title = "Railgun";

            // Check if mandatory directories exist.
            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir)) { 
                    Directory.CreateDirectory(dir);
                }
            }

            // Define filename
            string logFileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log";

            // Initialise the Log class
            Log.Init(directories[0] + "/" + logFileName);

            // Print start
            Log.Write(LogLevels.INFO, "Core", "Railgun");
#if DEBUG
            Log.Write(LogLevels.INFO, "Core", "This is a debug build, things might not turn out as expected.");
#endif
            Log.Write(LogLevels.INFO, "Core", "Starting...");
            Log.Write(LogLevels.INFO, "Core", "All output console is being logged to " + directories[0] + "/" + logFileName);

            // Loading the settings
            Log.Write(LogLevels.INFO, "Core", "Attempting to load settings...");

            // Make sure the config exists
            Config.Init();
            Config.Write("Meta", "last_started", DateTime.Now.ToString());

            // Attempt to get the protocol
            string protocol;

            try {
                protocol = Config.Read("Meta", "protocol");
            } catch {
                protocol = "";
            }

            if (protocol.Length < 1)
            {
                Log.Write(LogLevels.ERROR, "Core", "No protocol was set in the config, please set one!");
                Config.Write("Meta", "protocol", "");
                Shutdown();
            }

            // Building list of protocols
            Log.Write(LogLevels.INFO, "Core", "Building list of protocols...");
            ProtocolManager.BuildList(directories[3]);
            
            try {
                ProtocolManager.Loaded.TryGetValue(protocol, out Protocol);
            } catch {
                Log.Write(LogLevels.ERROR, "Core", "The specified protocol doesn't exist, please make sure you're using a protocol that exists in the Protocols folder.");
                Shutdown();
            }

            Log.Write(LogLevels.INFO, "Core", "Loading protocol wrapper...");
            Protocol.Open();

            // Set flood limit
            Log.Write(LogLevels.INFO, "Core", "Setting message limit...");
            try {
                Chat.messageLimit = int.Parse(Config.Read("Meta", "flood_limit"));
            } catch
            {
                Log.Write(LogLevels.WARNING, "Core", "No message limit was set, the default value of 10 seconds has been written to the config!");
                Chat.messageLimit = 2;
                Config.Write("Meta", "flood_limit", "2");
            }

            // Initialise user handler
            Log.Write(LogLevels.INFO, "Core", "Initialising Users handler...");
            Users.Init();
            Log.Write(LogLevels.INFO, "Core", "Adding ChatBot user...");
            Users.Add(-1, "ChatBot", "#9E8DA7", "0 0 0 0 0");

            // Load extensions
            Log.Write(LogLevels.INFO, "Core", "Loading extensions...");
            ExtensionManager.Load(directories[1]);

            // Check if required configuration variables exist
            if (Config.Read("Meta", "server").Length < 1)
            {
                Config.Write("Meta", "server", "localhost");
                Log.Write(LogLevels.WARNING, "Core", "A server variable was not set in the config!");
                Log.Write(LogLevels.INFO, "Core", "A placeholder was created, please make sure to update this to the correct server address!");
                Shutdown();
            }
            
            // Attempt to connect to chat server
            Log.Write(LogLevels.INFO, "Core", "Connecting to chat server...");

            // Create a new list
            List<string> auth = new List<string>();

            // Grab the auth section from the config
            List<KeyValuePair<string, string>> authSection = Config.Section("Auth");

            // Iterate over the entries the the Auth section of the config
            foreach (KeyValuePair<string, string> part in authSection)
            {
                auth.Add(part.Value);
            }

            sock = new Sock(Config.Read("Meta", "server"), auth.ToArray());

            // Wait for the connection
            while (sock.connected != true) { }

            // Define graceful close
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Shutdown);

            // Hold idle
            shutdown.WaitOne();
        }

        public static void Shutdown(object sender = null, ConsoleCancelEventArgs args = null)
        {
            // Log the thing
            Log.Write(LogLevels.INFO, "Core", "Stopping...");

            // Close sock chat connection if active
            try
            {
                if (sock.connected)
                {
                    sock.CloseConnection();
                    Log.Write(LogLevels.INFO, "Core", "Disconnected from chat.");
                }
            }
            catch { }
            
            Log.Write(LogLevels.INFO, "Core", "Good bye!");

            shutdown.Set();

            if (args != null)
            {
                args.Cancel = true;
            }

            // Exit the environment
            Environment.Exit(0);
        }
    }
}
