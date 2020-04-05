using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd.Parsing.Tokens
{
    public class EOCToken : Token
    {
        public EOCToken(int index)
        {
            SetPosition(index);
            SetSource("");
        }
    }
}
