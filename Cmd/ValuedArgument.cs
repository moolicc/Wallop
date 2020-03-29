using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    public class ValuedArgument : Argument
    {
        public Type ValueType { get; private set; }
        public object Value { get; private set; }

        public ValuedArgument(string name, char shortName, string helpText, string selectionGroup, Type valueType, object value)
            : base(name, shortName, helpText, selectionGroup)
        {
            ValueType = valueType;
            Value = value;
        }

        public ValuedArgument Set(string name = null, char shortName = '\0', string helpText = null, string selectionGroup = null, Type valueType = null, object value = null)
        {
            base.Set<Argument>(name, shortName, helpText, selectionGroup);

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
