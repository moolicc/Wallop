using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Wallop.Cmd
{
    public class ParseResults
    {
        //TODO: Merge this type with ParserOutput

        public static ParseResults Empty => new ParseResults(ParserOutput.SucceededOnEmpty, null);

        public bool IsEmpty => Output.Empty;

        public string Command { get; set; }

        public ParserOutput Output { get; private set; }

        public ReadOnlyDictionary<string, string> Options { get; private set; }


        public ParseResults(ParserOutput output)
            : this(output, "", null)
        {

        }

        public ParseResults(ParserOutput output, string command)
            : this(output, command, null)
        {

        }

        public ParseResults(ParserOutput output, string command, Dictionary<string, string> options)
        {
            if(options == null)
            {
                options = new Dictionary<string, string>();
            }
            Command = command;
            Output = output;
            Options = new ReadOnlyDictionary<string, string>(options);
        }

        public string this[string key]
        {
            get
            {
                return Options[key];
            }
        }

        public bool Flag(string flag) => Contains(flag);

        public bool Contains(string option)
        {
            foreach (var item in Options.Keys)
            {
                if(string.Equals(option, item, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
