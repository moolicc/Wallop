using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class BoolToken : IToken
    {
        public string Value { get; init; }

        public int Index { get; init; }
        public bool ValueB { get; init; }

        public BoolToken(int index, string value, bool valueB)
        {
            Value = value;
            Index = index;
            ValueB = valueB;
        }
    }
}
