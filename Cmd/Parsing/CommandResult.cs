using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd.Parsing
{
    public class CommandResult
    {
        public int CommandIndex { get; private set; }

        public Dictionary<string, object> Arguments { get; private set; }

        public CommandResult(int commandIndex, Dictionary<string, object> args)
        {
            CommandIndex = commandIndex;
            Arguments = new Dictionary<string, object>(args);
        }
    }
}
