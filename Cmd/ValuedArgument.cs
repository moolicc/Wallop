using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    public class ValuedArgument : Argument
    {
        public Type ValueType { get; private set; }
        public object Value { get; private set; }

        public ValuedArgument(string name, char shortName = '\0', string helpText = "", string selectionGroup = "", bool required = true, Type valueType = null, object value = null)
            : base(name, shortName, helpText, selectionGroup, required)
        {
            ValueType = valueType;
            Value = value;
        }

        public ValuedArgument Set(string name = null, char shortName = '\0', string helpText = null, string selectionGroup = null, bool? required = null, Type valueType = null, object value = null)
        {
            base.Set<Argument>(name, shortName, helpText, selectionGroup, required);

            if (valueType != null)
            {
                ValueType = valueType;
            }
            if(value != null)
            {
                Value = value;
            }

            return this;
        }
    }
}
