using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd.Parsing
{
    public class CommandResult
    {
        public int CommandIndex { get; private set; }
        public string CommandName { get; private set; }
        public string Selector { get; private set; }


        public Dictionary<string, object> Arguments { get; private set; }


        public CommandResult(int commandIndex, string commandName, string selector, Dictionary<string, object> args)
        {
            CommandIndex = commandIndex;
            CommandName = commandName;
            Selector = selector;
            Arguments = new Dictionary<string, object>(args);
        }

        internal CommandResult(int commandIndex, Dictionary<string, object> args, CommandTable commandTable)
        {
            CommandIndex = commandIndex;
            Arguments = new Dictionary<string, object>(args);
            (CommandName, Selector) = commandTable.DestructCommandIndex(commandIndex);
        }
    }
}
