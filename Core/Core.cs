using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Extensions;

namespace Railgun
{
    public static class Core
    {
        private static ManualResetEvent shutdown = new ManualResetEvent(false); // Thing to keep the console window running
        private static DateTime startTime = DateTime.Now; // For getting the application uptime

        // Getting the uptime
        public static double GetUptime()
        {
            return (DateTime.Now - startTime).TotalSeconds;
        }
        
        // Main function
        public static void Main(string[] args)
        {
            string[] directories = new string[] {
                Constants.Directories.LOGS,
                Constants.Directories.EXTENSIONS,
                Constants.Directories.DATA
            };

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
            Log.Init(Constants.Directories.LOGS + "/" + logFileName);

#if DEBUG
            Log.Write(LogLevels.INFO, "Core", "This is a debug build, things might not turn out as expected.");
#endif
            Log.Write(LogLevels.INFO, "Core", "Starting...");
            Log.Write(LogLevels.INFO, "Core", "All output console is being logged to " + Constants.Directories.LOGS + "/" + logFileName);

            // Loading the settings
            Log.Write(LogLevels.INFO, "Core", "Attempting to load settings...");

            // Make sure the config exists
            Config.Init();
            Config.Write("Meta", "last_started", DateTime.Now.ToString());

            // Attempt to get the protocol
            /*string protocol = Config.Read("Meta", "protocol");
            
            if (protocol == null || protocol.Length < 1)
            {
                Log.Write(LogLevels.ERROR, "Core", "No protocol was set in the config, please set one!");
                Config.Write("Meta", "protocol", "");
                Shutdown();
            }
            
            // Building list of protocols
            Log.Write(LogLevels.INFO, "Core", "Building list of protocols...");
            
            if (!ProtocolManager.Loaded.TryGetValue(protocol, out Protocol)) {
                Log.Write(LogLevels.ERROR, "Core", "The specified protocol doesn't exist, please make sure you're using a protocol that exists in the Protocols folder.");
                Shutdown();
            }

            Log.Write(LogLevels.INFO, "Core", "Loading protocol wrapper...");
            Protocol.Open();
            */
            Protocols.Server.Open(Protocols.Protocol.SOCKLEGACY);

            // Initialise user handler
            Log.Write(LogLevels.INFO, "Core", "Adding Server user...");
            User bot = new User(-1, "Server", "inherit");
            Users.Add(bot);

            // Load extensions
            Log.Write(LogLevels.INFO, "Core", "Loading extensions...");
            ExtensionManager.Load(Constants.Directories.EXTENSIONS);

            string server = Config.Read("Meta", "server");

            // Check if required configuration variables exist
            if (server == null || server.Length < 1)
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
            Dictionary<string, string> authSection = Config.Section("Auth");

            // Iterate over the entries the the Auth section of the config
            foreach (KeyValuePair<string, string> part in authSection)
            {
                auth.Add(part.Value);
            }

            /*sock = new Sock(Config.Read("Meta", "server"), auth.ToArray());

            // Wait for the connection
            while (sock.connected != true) { }*/

            // Define graceful close
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Shutdown);

            // Hold idle
            shutdown.WaitOne();
        }

        public static void Shutdown(object sender = null, ConsoleCancelEventArgs args = null)
        {
            // Log the thing
            Log.Write(LogLevels.INFO, "Core", "Stopping...");

            Protocols.Server.Close();
            
            Log.Write(LogLevels.INFO, "Core", "Good bye!");

            GC.Collect();

            shutdown.Set();

            if (args != null)
            {
                args.Cancel = true;
            }
        }
    }
}
