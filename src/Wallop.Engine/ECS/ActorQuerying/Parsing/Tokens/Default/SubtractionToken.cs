using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class SubtractionToken : IToken
    {
        public string Value => "-";

        public int Index { get; init; }

        public SubtractionToken(int index)
        {
            Index = index;
        }
    }
}
