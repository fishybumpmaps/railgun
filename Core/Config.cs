using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Linq;

namespace Core
{
    public static class Config
    {
        // Path to INI file
        public static readonly string path = Directory.GetCurrentDirectory() + "/Config.ini";
        
        // Initialise
        public static void Init()
        {
            Log.Write(0, "Config", "Initialising configuration.");

            // Check if the file exists
            if (!File.Exists(path))
            {
                Log.Write(0, "Config", "Configuration file doesn't exists, creating a new file.");
                File.Create("Config.ini");
            }
        }

        // Open the config file
        private static string RawRead()
        {
            return File.ReadAllText(path);
        }

        // Write data to the ini file
        public static void Write(string section, string key, string value)
        {
            // Perform a raw read
            string Raw = RawRead();

            // Escape the section name
            section = Regex.Escape(section);

            // Fetch the region using a regex
            string SectionRaw = Regex.Match(Raw, string.Format(@"^(?<=\[{0}\]\r\n)(?:(?!^\[).)*(?=\r\n)", section), RegexOptions.Multiline | RegexOptions.Singleline).ToString();

            // Check if the section exists
            if (SectionRaw.Length < 1)
            {
                Raw += string.Format("[{0}]\r\n", section);
                Raw += string.Format("{0}={1}\r\n", key, value);
            }
            else {
                // Check if the value exists
                string EntryRaw = Regex.Match(SectionRaw, string.Format(@"^{0}[^;\r\n]*", key), RegexOptions.Multiline).ToString();

                // Check if the entry exists
                if (EntryRaw.Length < 1)
                {
                    SectionRaw += string.Format("\r\n{0}={1}", key, value);
                }
                else
                {
                    SectionRaw = Regex.Replace(SectionRaw, string.Format(@"^{0}[^;\r\n]*", key), string.Format("{0}={1}", key, value), RegexOptions.Multiline);
                }

                // Replace the section with the newer section
                Raw = Regex.Replace(Raw, string.Format(@"^(?<=\[{0}\]\r\n)(?:(?!^\[).)*(?=\r\n)", section), SectionRaw, RegexOptions.Multiline | RegexOptions.Singleline);
            }

            // Save the file
            File.WriteAllText(path, Raw);
        }

        // Read data from the ini file
        public static string Read(string section, string key)
        {
            // Get the section we need
            List<KeyValuePair<string, string>> CSection = Section(section);

            // Create the entry variable
            KeyValuePair<string, string> entry;

            // Use Linq to get the entry
            try {
                entry = CSection.Single(get => get.Key == key);
            } catch
            {
                return "";
            }

            // Return the value
            return entry.Value;
        }

        // Get a section
        public static List<KeyValuePair<string, string>> Section(string section)
        {
            // Perform a raw read
            string Raw = RawRead();

            // Escape the section name
            section = Regex.Escape(section);
            
            // Fetch the region using a regex
            Match SectionRaw = Regex.Match(Raw, string.Format(@"^(?<=\[{0}\]\r\n)(?:(?!^\[).)*(?=\r\n)", section), RegexOptions.Multiline | RegexOptions.Singleline);

            // Split the entries up
            MatchCollection SectionEntries = Regex.Matches(SectionRaw.ToString(), @"^[^;\s][^;\r\n]*", RegexOptions.Multiline);

            // Create result list
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            
            // Iterate over the matches
            foreach (Match entryRaw in SectionEntries)
            {
                // Convert the entry to string and split at the =
                string[] entry = entryRaw.ToString().Split('=');

                // Add KeyValuepairs for each entry
                result.Add(
                    new KeyValuePair<string, string>(
                        entry[0],
                        entry[1]
                    )
                );
            }

            // Return the result
            return result;
        }
    }
}
