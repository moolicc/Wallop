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

        public string GenerateHelpText()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var item in Commands)
            {
                builder.AppendLine($"{item.Name}");
                builder.AppendLine($"  {item.HelpText}").AppendLine();
            }

            return builder.ToString().TrimEnd();
        }
    }
}
