using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public enum LogicalOperators
    {
        Or,
        And,
        Not
    }

    public class LogicalToken : IToken
    {
        public string Value { get; init; }
        public int Index { get; init; }

        public LogicalOperators Operator { get; init; }

        public LogicalToken(int index, LogicalOperators op)
        {
            Index = index;
            Operator = op;

            switch (op)
            {
                case LogicalOperators.Or:
                    Value = "or";
                    break;
                case LogicalOperators.And:
                    Value = "and";
                    break;
                case LogicalOperators.Not:
                    Value = "not";
                    break;
                default:
                    Value = "";
                    break;
            }
        }
    }
}
