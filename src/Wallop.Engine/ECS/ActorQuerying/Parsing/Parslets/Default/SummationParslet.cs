using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions.Default;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Parslets.Default
{
    public class SummationParslet : BinaryOperatorParsletBase
    {
        public SummationParslet()
            : base(false)
        {

        }

        protected override IExpression Parse(QueryParser parser, IToken token, IExpression lhs, IExpression rhs)
        {
            if (token is Tokens.Default.AdditionToken)
            {
                return new AddExpression(lhs, rhs);
            }
            else if (token is Tokens.Default.SubtractionToken)
            {
                return new SubtractExpression(lhs, rhs);
            }

            throw new InvalidOperationException("Unknown arithmetic token reached.");
        }
    }
}
