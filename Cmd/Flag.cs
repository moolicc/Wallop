using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    public class Flag : Argument
    {
        public bool Value { get; private set; }

        public Flag(string name, char shortName = '\0', string helpText = "", string selectionGroup = "", bool required = true, bool value = false)
            : base(name, shortName, helpText, selectionGroup, required)
        {
            Value = value;
        }

        public Flag Set(string name = null, char shortName = '\0', string helpText = null, string selectionGroup = null, bool? required = null, bool? value = null)
        {
            base.Set<Argument>(name, shortName, helpText, selectionGroup, required);

            if(value.HasValue)
            {
                Value = value.Value;
            }

            return this;
        }
    }
}
