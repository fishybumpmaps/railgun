using System;
using System.IO;

namespace Core
{
    public static class Log
    {
        // Log writer
        private static StreamWriter logWriter;

        // Initialiser
        public static void Init(string file)
        {
            // Create stream writer
            logWriter = new StreamWriter(file);

            // Enable auto flush
            logWriter.AutoFlush = true;
        }

        // Logging function
        public static void Write(LogLevels type, string module, string text)
        {
            // Define some variables
            string eventType,
                   logLine = "[{0}] [{1}] [{2}] {3}";
            string[] logArgs;

            // Get the right textual value for the type
            switch (type)
            {
                case LogLevels.ERROR:
                    eventType = "ERROR";
                    break;

                case LogLevels.WARNING:
                    eventType = "WARNING";
                    break;

                default:
                    eventType = "INFO";
                    break;
            }

            logArgs = new string[4] {
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                module,
                eventType,
                text
            };

            // Create a log line
            logLine = string.Format(logLine, logArgs);

            // Log if the logWriter isn't null
            try
            {
                logWriter.WriteLine(logLine);
            }
            catch { }

            // Write it to the console
            Console.WriteLine(logLine);
        }
    }
}
