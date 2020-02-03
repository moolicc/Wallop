using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    public class Command
    {
        public string Name { get; set; }
        public Action<ParseResults> InvocationTarget { get; set; }
        public List<Option> Options { get; private set; }


        public Command()
        {
            Name = "";
            Options = new List<Option>();
        }

        public static Command Create(string name = "")
        {
            return new Command { Name = name };
        }

        public Command SetName(string name)
        {
            Name = name;
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
    }
}
