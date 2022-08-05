using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class FirstToken : IToken
    {
        public string Value { get; init; }

        public int Index { get; init; }

        public FirstToken(int index, string keyword = "?")
        {
            Index = index;
            Value = keyword;
        }
    }
}
