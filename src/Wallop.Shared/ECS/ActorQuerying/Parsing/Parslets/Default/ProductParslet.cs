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
    public class ProductParslet : BinaryOperatorParsletBase
    {
        public ProductParslet()
            : base(false)
        {

        }

        protected override IExpression Parse(QueryParser parser, IToken token, IExpression lhs, IExpression rhs)
        {
            if (token is ProductToken aToken)
            {
                return new MultiplyExpression(lhs, rhs);
            }
            else if (token is DivideToken)
            {
                return new DivideExpression(lhs, rhs);
            }

            throw new InvalidOperationException("Unknown product token reached.");
        }
    }
}
