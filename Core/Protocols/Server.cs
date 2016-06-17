using System;

namespace Railgun.Protocols
{
    public static class Server
    {
        private static IProtocol ActiveProtocol;

        public static void Open(Protocol protocol)
        {
            switch (protocol) {
                case Protocol.SOCKLEGACY:
                    ActiveProtocol = new SockLegacy.Protocol();
                    break;
            }
        }

        public static void Close()
        {
            ActiveProtocol = null;
            GC.Collect(); // to invoke the destructor
        }

        public static void SendMessage(string message)
        {
            if (ActiveProtocol != null)
            {
                ActiveProtocol.SendMessage(message);
            }
        }

        public static int State()
        {
            if (ActiveProtocol != null)
            {
                return ActiveProtocol.State();
            }

            return 0;
        }
    }
}
