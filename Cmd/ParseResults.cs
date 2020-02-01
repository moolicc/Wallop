using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    class ParseResults
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
    }
}
