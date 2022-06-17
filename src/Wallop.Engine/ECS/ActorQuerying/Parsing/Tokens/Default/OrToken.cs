using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class OrToken : IToken
    {
        public string Value { get; init; }
        public int Index { get; init; }

        public OrToken(int index, string value = "or")
        {
            Value = value;
            Index = index;
        }
    }
}
