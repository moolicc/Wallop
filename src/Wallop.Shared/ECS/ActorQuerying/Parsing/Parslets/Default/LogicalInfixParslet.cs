using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions.Default;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens.Default;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Parslets.Default
{
    public class LogicalInfixParslet : BinaryOperatorParsletBase
    {
        protected override IExpression Parse(QueryParser parser, IToken token, IExpression lhs, IExpression rhs)
        {
            if (token is AndToken aToken)
            {
                return new AndExpression(lhs, rhs);
            }
            else if (token is OrToken oToken)
            {
                return new OrExpression(lhs, rhs);
            }
            throw new InvalidOperationException("Unexpected token.");
        }
    }
}
