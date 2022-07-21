using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.ECS.ActorQuerying.Parsing.Tokens;

namespace Wallop.ECS.ActorQuerying.Parsing.Parslets.Default
{
    public class CallParslet : BinaryOperatorParsletBase
    {
        public override IExpression Parse(QueryParser parser, IToken token, IExpression lhs)
        {
            var expression = new Expressions.Default.InvocationExpression(lhs);

            if (!parser.Match<Tokens.Default.RParenToken>())
            {
                do
                {
                    expression.Arguments.Add(parser.ParseNextExpression(0));
                } while (parser.Match<Tokens.Default.CommaToken>());

                parser.Consume<Tokens.Default.RParenToken>(false);
            }

            return expression;
        }

        protected override IExpression Parse(QueryParser parser, IToken token, IExpression lhs, IExpression rhs)
        {
            throw new NotImplementedException();
        }
    }
}
