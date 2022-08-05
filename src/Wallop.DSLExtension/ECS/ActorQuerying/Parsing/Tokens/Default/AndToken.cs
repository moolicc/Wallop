using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class AndToken : IToken
    {
        public string Value { get; init; }
        public int Index { get; init; }

        public AndToken(int index, string value = "and")
        {
            Value = value;
            Index = index;
        }
    }
}
