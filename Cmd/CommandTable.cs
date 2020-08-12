using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;

namespace Wallop.Cmd
{
    public class CommandTable
    {
        // Flat list of all selectors in use. Contains no duplicates.
        public string[] Selectors { get; private set; }

        public Command[] Commands { get; private set; }

        // Command signature map. Maps arguments associated with a command to their selector.
        // Dictionary<sigkey, (command index, args associated with selectort)>
        private Dictionary<string, (int CommandIndex, List<Argument> Signature)> _commandSigMap;

        private CommandTable()
        {
            _commandSigMap = new Dictionary<string, (int, List<Argument>)>();
        }

        public string FindSigKey(int commandIndex)
        {
            foreach (var item in _commandSigMap)
            {
                if (item.Value.CommandIndex == commandIndex)
                {
                    return item.Key;
                }
            }
            return string.Empty;
        }

        public (string CommandName, string Selector) DestructCommandIndex(int commandIndex)
        {
            foreach (var item in _commandSigMap)
            {
                if(item.Value.CommandIndex == commandIndex)
                {
                    var parts = item.Key.Split('`');
                    return (parts[0], parts[1]);
                }
            }
            throw new KeyNotFoundException($"Command with index {commandIndex} not found.");
        }

        public static CommandTable FromSet(CommandSet commandSet)
        {
            CommandTable table = new CommandTable();
            table.BuildTable(commandSet);
            return table;
        }

        private static string BuildSigKey(string commandName, string selectorName)
        {
            return $"{commandName}`{selectorName}";
        }

        private void BuildTable(CommandSet commandSet)
        {
            List<string> selectors = new List<string>();

            int i = 0;
            foreach (var command in commandSet.Commands.Values)
            {
                foreach (var arg in command.Arguments)
                {
                    if (!Selectors.Contains(arg.SelectionGroup))
                    {
                        selectors.Add(arg.SelectionGroup);
                    }


                    string sigKey = BuildSigKey(command.Name, arg.SelectionGroup);
                    if (!_commandSigMap.TryGetValue(sigKey, out var sigValue))
                    {
                        sigValue = (i, new List<Argument>());
                        _commandSigMap.Add(sigKey, sigValue);
                    }

                    sigValue.Signature.Add(arg);
                }

                i++;
            }

            Commands = commandSet.Commands.Values.ToArray();
            Selectors = selectors.ToArray();
        }

        internal CommandCandidate FindCandidate(string commandName, string selector)
        {
            var result = new CommandCandidate(BuildSigKey(commandName, selector));

            if (_commandSigMap.TryGetValue(result.SignatureKey, out var sigValue))
            {
                result.CommandIndex = sigValue.CommandIndex;
                result.Arguments = sigValue.Signature.ToArray();
                result.Found = true;
            }

            return result;
        }

        internal class CommandCandidate
        {
            public string SignatureKey { get; }
            public bool Found { get; set; }
            public int CommandIndex { get; set; }
            public Argument[] Arguments { get; set; }

            public CommandCandidate(string signatureKey)
            {
                SignatureKey = signatureKey;
                Found = false;
                CommandIndex = -1;
                Arguments = new Argument[0];
            }
        }
    }
}
