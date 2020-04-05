using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd.Parsing.Tokens
{
    public class ArgValueToken<T> : NameToken
    {
        public Type ValueType { get; }
        public T ActualValue { get; }


        public ArgValueToken(string value, Type valueType, T actualValue)
            : base(value)
        {
            ValueType = valueType;
            ActualValue = actualValue;
        }
    }
}
