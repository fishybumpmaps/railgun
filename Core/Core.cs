using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Extensions;

namespace Core
{
    class Core
    {
        public static Ini settings; // INI interface container
        public static Sock sock; // Sock interface container
        public static string[] directories = { "Logs", "Extensions", "Data" }; // Logs directory name
        private static ManualResetEvent shutdown = new ManualResetEvent(false); // Thing to keep the console window running
        public static ICollection<IExtension> Extensions; // Extension container
        private static DateTime startTime = DateTime.Now; // For getting the application uptime

        // Getting the uptime
        public static double GetUptime()
        {
            return (DateTime.Now - startTime).TotalSeconds;
        }

        // Extensions (re)loader
        public static void LoadExtensions()
        {
            Log.Write(0, "Core", "Loading extensions...");
            Extensions = ExtensionLoader.LoadExtensions(directories[1]);
            Log.Write(0, "Core", "Loaded extensions!");
        }

        // Main function
        static void Main(string[] args)
        {
            // Set console title
            Console.Title = "Shinoa";

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
            Log.Write(0, "Core", "Shinoa");
#if DEBUG
            Log.Write(0, "Core", "This is a debug build, things might not turn out as expected.");
#endif
            Log.Write(0, "Core", "Starting...");
            Log.Write(0, "Core", "All output console is being logged to " + directories[0] + "/" + logFileName);

            // Loading the settings
            Log.Write(0, "Core", "Attempting to load settings...");

            // Initialise INI class
            try {
                settings = new Ini(Directory.GetCurrentDirectory() + "/Config.ini");
            } catch {
                Log.Write(0, "Core", "Settings file doesn't exist. Attempting to create a blank one.");
            }
            settings.Write("Meta", "last_started", DateTime.Now.ToString());
            Log.Write(0, "Core", "Loaded settings!");

            // Set flood limit
            Log.Write(0, "Core", "Setting message limit...");
            try {
                Chat.messageLimit = int.Parse(settings.Read("Meta", "flood_limit"));
            } catch
            {
                Log.Write(1, "Core", "No message limit was set, the default value of 10 seconds has been written to the config!");
                Chat.messageLimit = 10;
                settings.Write("Meta", "flood_limit", "10");
            }

            // Initialise user handler
            Log.Write(0, "Core", "Initialising Users handler...");
            Users.Init();
            Log.Write(0, "Core", "Adding ChatBot user...");
            Users.Add(-1, "ChatBot", "#9E8DA7", "0 0 0 0 0");
            Log.Write(0, "Core", "Users handler initialised.");

            // Loading extensions
            LoadExtensions();

            // Check if required configuration variables exist
            if (settings.Read("Meta", "server").Length < 1)
            {
                settings.Write("Meta", "server", "localhost");
                Log.Write(1, "Core", "A server variable was not set in the config!");
                Log.Write(0, "Core", "A placeholder was created, please make sure to update this to the correct server address!");
            }
            
            // Attempt to connect to chat server
            Log.Write(0, "Core", "Connecting to chat server...");
            sock = new Sock(settings.Read("Meta", "server"), settings.Section("Auth", true).ToArray());

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
            Log.Write(0, "Core", "Stopping...");

            // Close sock chat connection if active
            try
            {
                if (sock.connected)
                {
                    sock.CloseConnection();
                    Log.Write(0, "Core", "Disconnected from chat.");
                }
            }
            catch { }

            Log.Write(0, "Core", "Destructing loaded extensions.");

            foreach (IExtension extension in Extensions)
            {
                extension.Destruct();
            }

            Log.Write(0, "Core", "Good bye!");

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
