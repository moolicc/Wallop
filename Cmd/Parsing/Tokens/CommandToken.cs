using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd.Parsing.Tokens
{
    public class CommandToken : NameToken
    {
        public CommandToken(string name)
            : base(name)
        {
        }
    }
}
