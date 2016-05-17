using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Protocol;
using Core;

namespace SockLegacy
{
    public class Protocol : IProtocol
    {
        public void Close()
        {
        }

        public string Name()
        {
            return "SockLegacy";
        }

        public void Open()
        {
            // Grab the auth section from the config
            List<KeyValuePair<string, string>> authSection = Config.Section("Auth");

            // Iterate over the entries the the Auth section of the config
            foreach (KeyValuePair<string, string> part in authSection)
            {
                Log.Write(LogLevels.INFO, "SockLegacy", part.Key);
            }
        }

        public void SendMessage(string text)
        {
        }

        public int State()
        {
            return 0;
        }
    }
}
