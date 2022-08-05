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
                if (iToken.Value.Contains('.'))
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
