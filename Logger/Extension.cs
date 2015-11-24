using Core;
using Extensions;

namespace Logger
{
    public class Extension : IExtension
    {
        public string Name
        {
            get
            {
                return "Logger";
            }
        }

        public void Initialise()
        {
            Log.Write(0, "Logger", "Initialised Chat Logger.");
            Logger.Start();
        }

        public void Destruct()
        {
            Logger.Stop();
        }

        public void Handle(string[] data)
        {
            Logger.Handle(data);
        }
    }
}
