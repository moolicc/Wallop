using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public enum GroupingOperators
    {
        LParen,
        RParen,
    }

    public class GroupingTokens : IToken
    {
        public string Value { get; init; }

        public int Index { get; init; }
        public GroupingOperators Operator { get; init; }

        public GroupingTokens(int index, GroupingOperators op)
        {
            Index = index;
            Operator = op;
        }
    }
}
