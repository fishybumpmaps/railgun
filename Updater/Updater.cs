using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Updater
{
    public class Updater
    {
        private string updateServer = "";
        private int lastRound = 0;

        public void Main(string server)
        {
            // Set server
            updateServer = server;
            Log.Write(0, "Updater", "Set update server to " + server + ".");

            // Dispatch the updater
            Dispatch();
            Log.Write(0, "Updater", "Dispatcher started.");
        }

        public void Dispatch()
        {

        }

        public void Check()
        {

        }
    }
}
