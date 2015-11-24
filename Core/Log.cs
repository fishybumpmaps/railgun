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
        public static void Write(int type, string module, string text)
        {
            // Define some variables
            string eventType, logLine;

            // Get the right textual value for the type
            switch (type)
            {
                case 2:
                    eventType = "ERROR";
                    break;
                case 1:
                    eventType = "WARNING";
                    break;
                default:
                    eventType = "INFO";
                    break;
            }

            // Create a log line
            logLine = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [" + module + "] [" + eventType + "] " + text;

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
