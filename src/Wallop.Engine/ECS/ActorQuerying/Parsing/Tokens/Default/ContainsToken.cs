using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class ContainsToken : IToken
    {
        public string Value { get; init; }

        public int Index { get; init; }

        public ContainsToken(int index, string keyword = "contains")
        {
            Value = keyword;
            Index = index;
        }

    }
}
