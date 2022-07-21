using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.ECS.ActorQuerying.Parsing.Tokens
{
    public class EndOfStreamToken : IToken
    {
        public string Value => "\0";

        public int Index { get; init; }

        public EndOfStreamToken(int index)
        {
            Index = index;
        }
    }
}
