using Wallop.Shared.ECS.ActorQuerying.Parsing;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens.Default;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Parslets.Default
{
    public class SummationPrefixParslet : PrefixParsletBase
    {

        public IExpression Parse(QueryParser parser, IToken token, IExpression lhs)
        {
            throw new NotImplementedException();
        }

        protected override IExpression Parse(QueryParser parser, IExpression right, IToken token)
        {
            if (token is AdditionToken aToken)
            {
                return new Expressions.Default.PositivifyExpression(right);
            }
            else if (token is SubtractionToken sToken)
            {
                return new Expressions.Default.NegationExpression(right);
            }
            throw new InvalidOperationException("Unexpected token.");
        }

        protected override IExpression ParseStandalone(QueryParser parser, IToken token)
        {
            throw new NotImplementedException();
        }
    }
}