using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd.Parsing.Tokens
{
    public class ArgValueToken : Token
    {
        public string Value { get; }


        public ArgValueToken(string value)
        {
            Value = value;
        }
    }
}
