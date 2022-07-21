using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.ECS.ActorQuerying.Parsing.Expressions.Default;
using Wallop.ECS.ActorQuerying.Parsing.Tokens;
using Wallop.ECS.ActorQuerying.Parsing.Tokens.Default;

namespace Wallop.ECS.ActorQuerying.Parsing.Parslets.Default
{
    public class IdentifierParslet : PrefixParsletBase
    {
        public IdentifierParslet()
            : base(true)
        {
        }

        protected override IExpression Parse(QueryParser parser, IExpression right, IToken token)
        {
            throw new NotImplementedException();
        }

        protected override IExpression ParseStandalone(QueryParser parser, IToken token)
        {
            if (token is IdentifierToken iToken)
            {
                if(iToken.Value.Contains('.'))
                {
                    var split = iToken.Value.Split('.');
                    return new MemberExpression(split[^1], split[..^1]);
                }
                return new MemberExpression(iToken.Value);
            }

            throw new InvalidOperationException("Unexpected token reached.");
        }
    }
}
