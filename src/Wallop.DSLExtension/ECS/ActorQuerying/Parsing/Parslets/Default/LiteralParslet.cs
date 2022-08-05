using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions.Default;
using Wallop.Shared.ECS.ActorQuerying.Parsing;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens.Default;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Parslets.Default
{
    public class LiteralParslet : PrefixParsletBase
    {
        public LiteralParslet()
            : base(true)
        {
        }

        protected override IExpression Parse(QueryParser parser, IExpression right, IToken token)
        {
            throw new NotImplementedException();
        }

        protected override IExpression ParseStandalone(QueryParser parser, IToken token)
        {
            if (token is StringToken sToken)
            {
                return new StringExpression(sToken.Value);
            }
            else if (token is IntToken iToken)
            {
                return new IntegerExpression(iToken.ValueI);
            }
            else if (token is RealToken rToken)
            {
                return new RealExpression(rToken.ValueF);
            }
            else if (token is BoolToken bToken)
            {
                return new BoolExpression(bToken.ValueB);
            }

            throw new InvalidOperationException("Unexpected token literal reached.");
        }
    }
}
