using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    public class CommandSet
    {
        public Dictionary<string, Command> Commands { get; private set; }

        public CommandSet()
        {
            Commands = new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase);
        }

    }
}
