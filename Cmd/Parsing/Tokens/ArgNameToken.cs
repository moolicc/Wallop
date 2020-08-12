using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd.Parsing.Tokens
{
    public class ArgNameToken : NameToken
    {
        public bool IsShort { get; }

        public ArgNameToken(string name)
            : this(name, name.Length == 1)
        {
        }

        public ArgNameToken(string name, bool shortName)
            : base(name)
        {
            IsShort = shortName;
        }
    }
}
