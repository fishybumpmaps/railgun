using System.Collections.Generic;

namespace Responses
{
    public class Trigger
    {
        public string[] Triggers { get; set; }

        public List<List<string>> Responses { get; set; }

        public Trigger(string[] Triggers, List<List<string>> Responses)
        {
            this.Triggers = Triggers;
            this.Responses = Responses;
        }
    }
}
