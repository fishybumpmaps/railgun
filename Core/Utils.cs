using System;

namespace Core
{
    public static class Utils
    {
        public static DateTime EpochFrom(long unix)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unix);
        }
        public static string[] GetDirectories()
        {
            // ^^;
            return Core.directories;
        }
    }
}
