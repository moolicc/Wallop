using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class StringToken : IToken
    {
        public string Value { get; init; }

        public int Index { get; init; }

        public StringToken(int index, string value)
        {
            Value = value;
            Index = index;
        }
    }
}
