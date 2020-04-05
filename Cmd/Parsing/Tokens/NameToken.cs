using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd.Parsing.Tokens
{
    public class NameToken : Token
    {
        public string Name { get; private set; }

        public NameToken(string name)
        {
            Name = name;
        }
    }
}
