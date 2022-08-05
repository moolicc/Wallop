using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens.Default;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Parslets.Default
{
    public class CallParslet : BinaryOperatorParsletBase
    {
        public override IExpression Parse(QueryParser parser, IToken token, IExpression lhs)
        {
            var expression = new Expressions.Default.InvocationExpression(lhs);

            if (!parser.Match<RParenToken>())
            {
                do
                {
                    expression.Arguments.Add(parser.ParseNextExpression(0));
                } while (parser.Match<CommaToken>());

                parser.Consume<RParenToken>(false);
            }

            return expression;
        }

        protected override IExpression Parse(QueryParser parser, IToken token, IExpression lhs, IExpression rhs)
        {
            throw new NotImplementedException();
        }
    }
}
