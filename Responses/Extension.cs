using Core;
using Extensions;

namespace Responses
{
    public class Extension : IExtension
    {
        public string Name
        {
            get
            {
                return "Responses";
            }
        }

        public void Initialise()
        {
            Responses.Load();
            Log.Write(0, "Responses", "Initialised Responses extension.");
        }

        public void Destruct()
        {
        }

        public void Handle(string[] data)
        {
            try {
                // Do nothing if we sent the message
                if (int.Parse(data[2]) == Chat.userId)
                    return;
            } catch { }

            // Forward to the handler
            Responses.Handle(data);
        }
    }
}
