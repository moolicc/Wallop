using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public enum ComparisonModes
    {
        Equal,
        NotEqual,
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual,
    }

    public class ComparisonToken : IToken
    {
        public string Value { get; init; }
        public int Index { get; init; }

        public ComparisonModes Operator { get; init; }

        public ComparisonToken(int index, string value, ComparisonModes op)
        {
            Index = index;
            Value = value;
            Operator = op;
        }
    }
}
