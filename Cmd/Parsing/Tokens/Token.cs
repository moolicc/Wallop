using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd.Parsing.Tokens
{
    public abstract class Token
    {
        public int Position { get; private set; }
        public string Source { get; private set; }


        public Token()
        {
            Position = -1;
            Source = "";
        }


        public void SetPosition(int position)
        {
            Position = position;
        }

        public void SetSource(string source)
        {
            Source = source;
        }
    }
}
