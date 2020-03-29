using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    public class Flag : Argument
    {
        public bool Value { get; private set; }

        public Flag(string name, char shortName, string helpText, string selectionGroup, bool value)
            : base(name, shortName, helpText, selectionGroup)
        {
            Value = value;
        }

        public Flag Set(string name = null, char shortName = '\0', string helpText = null, string selectionGroup = null, bool? value = null)
        {
            base.Set<Argument>(name, shortName, helpText, selectionGroup);

            if(value.HasValue)
            {
                Value = value.Value;
            }

            return this;
        }
    }
}
