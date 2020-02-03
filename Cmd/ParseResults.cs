using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    public class ParseResults
    {
        public string Command { get; set; }

        public Dictionary<string, string> Options { get; private set; }

        public ParseResults()
        {
            Command = "";
            Options = new Dictionary<string, string>();
        }

        public string this[string key]
        {
            get
            {
                return Options[key];
            }
        }

        public bool Flag(string flag) => Contains(flag);

        public bool Contains(string option)
        {
            foreach (var item in Options.Keys)
            {
                if(string.Equals(option, item, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
