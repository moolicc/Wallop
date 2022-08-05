using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens.Default
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
