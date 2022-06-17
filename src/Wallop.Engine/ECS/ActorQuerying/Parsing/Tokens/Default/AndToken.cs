using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens.Default
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
