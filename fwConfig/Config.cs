using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace fwConfig
{
    public class Config
    {
        private Dictionary<string, Dictionary<string, string>> Configuration;
        private Dictionary<int, string> Comments;
        private string ConfigFile;

        public Config(string filename = null)
        {
            Configuration = new Dictionary<string, Dictionary<string, string>>();
            Comments = new Dictionary<int, string>();

            ConfigFile = filename;

            if (ConfigFile != null)
            {
                if (!File.Exists(ConfigFile))
                {
                    FileStream fileCreationThrowaway = File.Create(filename);
                    fileCreationThrowaway.Close();
                }
                
                ParseFile(ConfigFile);
            }
        }

        private void ParseFile(string file)
        {
            string[] lines = File.ReadAllLines(file);
            int lineNo = -1;

            string sectionName = null;
            string key = "";
            string value = "";

            Dictionary<string, string> currentSection = new Dictionary<string, string>();
            
            foreach (string line in lines)
            {
                // skip empty lines
                if (line.Length < 1)
                {
                    continue;
                }

                ++lineNo;

                // comments
                if (line[0] == '#' || line[0] == ';' || (line.Length >= 2 && line[0] == '/' && line[1] == '/'))
                {
                    Comments.Add(lineNo, line);
                    continue;
                }

                // start a new section
                if (line[0] == '[' && line[line.Length - 1] == ']')
                {
                    string newSectionName = line.Substring(1, line.Length - 2);

                    if (sectionName != null)
                    {
                        Configuration.Add(sectionName, currentSection);

                        if (Configuration.ContainsKey(newSectionName))
                        {
                            currentSection = Configuration[newSectionName];
                            Configuration.Remove(newSectionName);
                        }
                        else
                        {
                            currentSection = new Dictionary<string, string>();
                        }

                        ++lineNo;
                    }

                    sectionName = newSectionName;
                    continue;
                }

                // entry
                if (line.IndexOf('=') > -1)
                {
                    List<string> splitLine = line.Split('=').ToList();

                    key = splitLine.ElementAt(0).Trim();

                    splitLine.RemoveAt(0);
                    
                    value = string.Join("=", splitLine.ToArray()).Trim();

                    if (key.Length >= 1 && value.Length >= 1)
                    {
                        currentSection.Add(key, value);
                    }
                }
            }

            // flush the final section
            if (sectionName != null && !Configuration.ContainsKey(sectionName))
            {
                Configuration.Add(sectionName, currentSection);
            }
        }

        public override string ToString()
        {
            List<string> lines = new List<string>();
            string finalString = null;
            
            foreach (KeyValuePair<string, Dictionary<string, string>> section in Configuration)
            {
                lines.Add(string.Format("[{0}]", section.Key.Trim()));

                foreach (KeyValuePair<string, string> variable in section.Value)
                {
                    lines.Add(string.Format("{0} = {1}", variable.Key.Trim(), variable.Value.Trim()));
                }

                lines.Add("");
            }

            foreach (KeyValuePair<int, string> comment in Comments)
            {
                if (comment.Key > lines.Count)
                {
                    break;
                }

                lines.Insert(comment.Key, comment.Value);
            }

            finalString = string.Join(Environment.NewLine, lines);

            return finalString;
        }

        public Dictionary<string, string> Section(string sectionName)
        {
            sectionName = sectionName.Trim();
            Dictionary<string, string> section = null;

            if (!Configuration.TryGetValue(sectionName, out section))
            {
                section = new Dictionary<string, string>();
            }

            return section;
        }

        public void Write()
        {
            File.WriteAllText(ConfigFile, ToString(), Encoding.UTF8);
        }

        public void Set(string sectionName, string key, string value, bool write = true)
        {
            sectionName = sectionName.Trim();
            key = key.Trim();
            value = value.Trim();

            Dictionary<string, string> section = Section(sectionName);

            section[key] = value;

            Configuration[sectionName] = section;

            if (write)
            {
                Write();
            }
        }

        public string Get(string sectionName, string key)
        {
            string value = null;

            key = key.Trim();
            Dictionary<string, string> section = Section(sectionName);

            if (section != null && section.ContainsKey(key))
            {
                value = section[key].Trim();
            }

            return value;
        }

        public void Remove(string sectionName, string key = null)
        {
            sectionName = sectionName.Trim();
            Dictionary<string, string> section = Section(sectionName);

            if (key == null || section.Count < 2)
            {
                Configuration.Remove(sectionName);
            } else
            {
                section.Remove(key.Trim());
                Configuration[sectionName] = section;
            }
        }

        public bool ContainsSection(string section)
        {
            return Configuration.ContainsKey(section.Trim());
        }

        public bool ContainsKey(string section, string key)
        {
            section = section.Trim();
            key = key.Trim();

            if (ContainsSection(section) && Section(section).ContainsKey(key))
            {
                return false;
            }

            return false;
        }
    }
}
