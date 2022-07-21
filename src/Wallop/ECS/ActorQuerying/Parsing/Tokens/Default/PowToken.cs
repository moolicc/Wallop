using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class PowToken : IToken
    {
        public string Value => "^";

        public int Index { get; init; }

        public PowToken(int index)
        {
            Index = index;
        }
    }
}
