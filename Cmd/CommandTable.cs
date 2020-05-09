using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    public class CommandTable
    {
        public List<string> Selectors { get; private set; }

        private CommandTable()
        {
            Selectors = new List<string>();
        }

        public static CommandTable FromSet(CommandSet commandSet)
        {
            CommandTable table = new CommandTable();
            table.BuildTable(commandSet);
            return table;
        }

        private void BuildTable(CommandSet commandSet)
        {
            foreach (var command in commandSet.Commands.Values)
            {
                foreach (var arg in command.Arguments)
                {
                    if(!Selectors.Contains(arg.SelectionGroup))
                    {
                        Selectors.Add(arg.SelectionGroup);
                    }
                }
            }
        }
    }
}
