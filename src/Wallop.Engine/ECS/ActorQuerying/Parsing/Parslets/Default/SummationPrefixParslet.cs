using Wallop.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.ECS.ActorQuerying.Parsing.Tokens;

namespace Wallop.ECS.ActorQuerying.Parsing.Parslets.Default
{
    public class SummationPrefixParslet : PrefixParsletBase
    {

        public IExpression Parse(QueryParser parser, IToken token, IExpression lhs)
        {
            throw new NotImplementedException();
        }

        protected override IExpression Parse(QueryParser parser, IExpression right, IToken token)
        {
            if(token is Tokens.Default.AdditionToken aToken)
            {
                return new Expressions.Default.PositivifyExpression(right);
            }
            else if(token is Tokens.Default.SubtractionToken sToken)
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