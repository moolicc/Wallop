using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.Queries;
using Wallop.Shared.ECS.ActorQuerying.Queries.Default;
using Wallop.Shared.ECS.ActorQuerying.Parsing;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens.Default;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Parslets.Default
{
    public class BasicCollectionParslet : PrefixParsletBase
    {
        public BasicCollectionParslet()
            : base(true)
        {
        }

        protected override IExpression ParseStandalone(QueryParser parser, IToken token)
        {
            if (token is AllToken)
            {
                return new AllQuery();
            }
            if (token is FirstToken)
            {
                return new FirstQuery();
            }
            if (token is LastToken)
            {
                return new LastQuery();
            }
            throw new InvalidOperationException("Unexpected token for CollectionParslet.");
        }

        protected override IExpression Parse(QueryParser parser, IExpression right, IToken token)
        {
            throw new NotImplementedException();
        }
    }
}