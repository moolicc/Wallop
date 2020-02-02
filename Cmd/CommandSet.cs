using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    public class CommandSet
    {
        public List<Command> Commands { get; private set; }

        public CommandSet()
        {
            Commands = new List<Command>();
        }
    }
}
