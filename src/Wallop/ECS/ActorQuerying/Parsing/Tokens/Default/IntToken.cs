using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class IntToken : IToken
    {
        public string Value { get; init; }

        public int Index { get; init; }
        public int ValueI { get; init; }

        public IntToken(int index, string value)
        {
            Value = value;
            Index = index;
            ValueI = int.Parse(value);
        }
    }
}
