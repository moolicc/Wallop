using Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Parslets.Default
{
    public class LogicalPrefixParslet : PrefixParsletBase
    {

        public IExpression Parse(QueryParser parser, IToken token, IExpression lhs)
        {
            throw new NotImplementedException();
        }

        protected override IExpression Parse(QueryParser parser, IExpression right, IToken token)
        {
            if(token is Tokens.Default.NotToken nToken)
            {
                return new Expressions.Default.NotExpression(right);
            }
            throw new InvalidOperationException("Unexpected token.");
        }

        protected override IExpression ParseStandalone(QueryParser parser, IToken token)
        {
            throw new NotImplementedException();
        }
    }
}