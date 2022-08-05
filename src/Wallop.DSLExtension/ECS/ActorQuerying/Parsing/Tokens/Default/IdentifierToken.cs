using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class IdentifierToken : IToken
    {
        public string Value { get; init; }

        public int Index { get; init; }

        public IdentifierToken(string value, int index)
        {
            Value = value;
            Index = index;
        }
    }
}
