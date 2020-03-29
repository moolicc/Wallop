using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    public abstract class Argument
    {
        public string Name { get; private set; }
        public char ShortName { get; private set; }
        public string HelpText { get; private set; }
        public string SelectionGroup { get; private set; }

        protected Argument(string name, char shortName, string helpText, string selectionGroup)
        {
            Name = name;
            ShortName = shortName;
            HelpText = helpText;
            SelectionGroup = selectionGroup;
        }

        public T Set<T>(string name = null, char shortName = '\0', string helpText = null, string selectionGroup = null) where T : Argument
        {
            if(!string.IsNullOrEmpty(name))
            {
                Name = name;
            }
            if(shortName != '\0')
            {
                ShortName = shortName;
            }
            if (!string.IsNullOrEmpty(helpText))
            {
                HelpText = helpText;
            }
            if (!string.IsNullOrEmpty(selectionGroup))
            {
                SelectionGroup = selectionGroup;
            }
            return (T)this;
        }
    }
}
