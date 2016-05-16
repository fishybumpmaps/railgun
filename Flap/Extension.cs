using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extensions;
using Core;

namespace Flap
{
    class Extension : IExtension
    {
        public void Init()
        {
            Log.Write(LogLevels.INFO, "Flap", "Flapstension 2.0 loaded.");
        }

        public void Kill()
        {
            Log.Write(LogLevels.INFO, "Flap", "Flapstension 2.0 unloaded.");
        }

        public string Name()
        {
            return "Flap";
        }
    }
}
