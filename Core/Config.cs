using System.Collections.Generic;

namespace Railgun
{
    public static class Config
    {
        public static fwConfig.Config config;
        
        public static void Init()
        {
            Log.Write(LogLevels.INFO, "Config", "Initialising configuration.");

            config = new fwConfig.Config("Config.ini");
        }

        private static string RawRead()
        {
            return config.ToString();
        }

        public static void Write(string section, string key, string value)
        {
            config.Set(section, key, value);
        }

        public static string Read(string section, string key)
        {
            return config.Get(section, key);
        }

        public static Dictionary<string, string> Section(string section)
        {
            return config.Section(section);
        }
    }
}
