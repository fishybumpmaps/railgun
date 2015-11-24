using Core;
using System;
using System.IO;

namespace Logger
{
    public static class Logger
    {
        public static bool active = false;
        private static StreamWriter logWriter;

        public static void Start()
        {
            // Check if already active
            if(active)
            {
                Log.Write(0, "Logger", "Logger is already active!");
                return;
            }
            // Create log filename
            string filename = "chat_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log";
            // Check if the destination directory exists
            if (!Directory.Exists(Utils.GetDirectories()[2] + "/Chatlogs/"))
            {
                Directory.CreateDirectory(Utils.GetDirectories()[2] + "/Chatlogs/");
            }
            // Create stream writer
            logWriter = new StreamWriter(Utils.GetDirectories()[2] + "/Chatlogs/" + filename);
            // Enable auto flush
            logWriter.AutoFlush = true;

            active = true;
            Log.Write(0, "Logger", "Logging chat to file " + filename + ".");
        }

        public static void Stop()
        {
            // Check if already active
            if (!active)
            {
                Log.Write(0, "Logger", "Logger isn't active!");
                return;
            }
            active = false;
            logWriter.Close();
            logWriter = null;
            Log.Write(0, "Logger", "Stopped logging the chat.");
        }

        public static void Handle(string[] data)
        {
            if(!active)
            {
                return;
            }
            logWriter.WriteLine(string.Join("\f", data));
        }
    }
}
