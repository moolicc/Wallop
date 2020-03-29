using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    public class Command
    {
        public string Name { get; set; }
        public string HelpText { get; set; }
        public Action<ParseResults> InvocationTarget { get; set; }
        public List<Option> Options { get; private set; }


        public Command()
        {
            Name = "";
            Options = new List<Option>();
        }

        public static Command Create(string name = "", string helpText = "")
        {
            return new Command { Name = name, HelpText = helpText, };
        }

        public Command SetName(string name)
        {
            Name = name;
            return this;
        }
        public Command SetHelpText(string helpText)
        {
            HelpText = helpText;
            return this;
        }

        public Command AddOption(Option option)
        {
            Options.Add(option);
            return this;
        }

        public Command AddOption(Action<Option> action)
        {
            var option = new Option();
            action(option);
            Options.Add(option);
            return this;
        }

        public Command Action(Action<ParseResults> action)
        {
            InvocationTarget = action;
            return this;
        }

        public string GetHelpText()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"{Name}");
            builder.AppendLine($"  {HelpText}").AppendLine();

            var options = Options.ToArray();
            Array.Sort(options, new Comparison<Option>((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.GroupSelection, b.GroupSelection)));
            Array.Reverse(options);
            string lastGroup = null;
            string indention = "  ";
            foreach (var option in options)
            {
                if (lastGroup == null && !string.IsNullOrEmpty(option.GroupSelection))
                {
                    indention = "  ";
                    lastGroup = option.GroupSelection;
                    builder.AppendLine($"{indention}{lastGroup}");
                    indention = "    ";
                }
                else if (lastGroup != option.GroupSelection)
                {
                    indention = "  ";
                    builder.AppendLine();
                    lastGroup = option.GroupSelection;
                    if (!string.IsNullOrEmpty(option.GroupSelection))
                    {
                        builder.AppendLine($"{indention}{lastGroup}");
                        indention = "    ";
                    }
                }
                if (option.IsSelector)
                {
                    continue;
                }

                builder.Append($"{indention}{option.Name}");
                if (!option.IsRequired)
                {
                    builder.Append("?");
                }
                builder.Append(" ");
                if (option.IsFlag)
                {
                    builder.Append("[flag]");
                }
                if (!string.IsNullOrEmpty(option.DefaultValue))
                {
                    builder.Append($" {option.DefaultValue}");
                }
                builder.AppendLine();
                if (!string.IsNullOrWhiteSpace(option.HelpText))
                {
                    builder.AppendLine($"{indention}  {option.HelpText}");
                }
            }

            return builder.ToString().TrimEnd();
        }
    }
}
