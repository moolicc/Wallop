using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class NotToken : IToken
    {
        public string Value { get; init; }
        public int Index { get; init; }

        public NotToken(int index, string value = "not")
        {
            Value = value;
            Index = index;
        }
    }
}
