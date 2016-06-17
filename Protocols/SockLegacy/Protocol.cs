using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Protocols.SockLegacy
{
    public class Protocol : IProtocol
    {
        public Protocol()
        {
            // Grab the auth section from the config
            Dictionary<string, string> authSection = new Dictionary<string, string>();//Config.Section("Auth");

            // Iterate over the entries the the Auth section of the config
            foreach (KeyValuePair<string, string> part in authSection)
            {
                Console.WriteLine(part.Key);
            }
        }

        ~Protocol()
        {
            Console.WriteLine("destructing");
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
