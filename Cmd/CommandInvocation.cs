using System;
using System.Collections.Generic;
using System.Text;
using Wallop.Cmd.Parsing;

namespace Wallop.Cmd
{
    public class CommandInvocation
    {
        public CommandSet CommandSet { get; private set; }

        public string Execute(ParseResults parsed)
        {
            //TODO: In the future, input/output from a command will be more complex. See note in Tokenizer.cs.
            string input = "";
            foreach (var item in parsed.Commands)
            {
                input = CommandSet.Commands[item.CommandName].Invoke(input, item.Selector, item.Arguments);
            }
            return input;
        }
    }
}
