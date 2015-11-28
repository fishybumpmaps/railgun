using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Core
{
    public class Ini
    {
        // Path to INI file
        public string path;

        // Writing a string to a key in a section
        [DllImport("kernel32")]
        private static extern void WritePrivateProfileString(string section, string key, string val, string filePath);

        // Reading a string from a key in a section
        [DllImport("kernel32")]
        private static extern void GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        // Get an entire section
        [DllImport("kernel32")]
        private static extern void GetPrivateProfileSection(string section, byte[] retVal, int size, string filePath);

        // Constructor
        public Ini(string path)
        {
            this.path = path;
        }

        // Write data to the ini file
        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, path);
        }

        // Read data from the ini file
        public string Read(string section, string key)
        {
            StringBuilder get = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", get, 255, path);
            return get.ToString();
        }

        // Get a section
        public List<string> Section(string section, bool valuesOnly = false)
        {
            // Create a buffer
            byte[] buffer = new byte[2048];

            // Get the sections from the ini file
            GetPrivateProfileSection(section, buffer, 2048, path);

            // Split the lines
            string[] content = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0');

            List<string> result = new List<string>();

            foreach (string entry in content)
            {
                result.Add(valuesOnly ? entry.Substring(entry.IndexOf('=') + 1) : entry);
            }

            return result;
        }
    }
}
