using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Railgun;
using System.Threading;

namespace RailgunConsole
{
    class Program
    {
        private static ManualResetEvent shutdown = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            // Set console title
            Console.Title = "Railgun";

            Core.Main(args);

            // Define graceful close
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Kill);

            // Hold idle
            shutdown.WaitOne();
        }

        public static void Kill(object sender = null, ConsoleCancelEventArgs args = null)
        {
            // Log the thing
            Log.Write(LogLevels.INFO, "Console", "Closing window...");

            shutdown.Set();

            if (args != null)
            {
                args.Cancel = true;
            }

#if DEBUG
            Console.ReadKey();
#endif

            // Exit the environment
            Environment.Exit(0);
        }
    }
}
