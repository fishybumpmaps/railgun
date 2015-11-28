using System.Collections.Generic;

namespace Responses
{
    public class Trigger
    {
        public string[] Message = new string[] { };
        public string[] Contains = new string[] { };
        public string[] StartWith = new string[] { };
        public string[] EndsWith = new string[] { };
        public List<List<string>> Responses = new List<List<string>>();
    }
}
