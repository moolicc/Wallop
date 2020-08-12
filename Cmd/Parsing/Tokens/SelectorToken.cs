using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd.Parsing.Tokens
{
    public class SelectorToken : NameToken
    {
        public SelectorToken(string selectorName)
            : base(selectorName)
        {
        }
    }
}
