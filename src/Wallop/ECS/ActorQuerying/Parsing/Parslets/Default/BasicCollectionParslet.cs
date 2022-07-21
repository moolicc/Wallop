using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.ECS.ActorQuerying.Parsing.Tokens;
using Wallop.ECS.ActorQuerying.Parsing.Tokens.Default;
using Wallop.ECS.ActorQuerying.Queries;
using Wallop.ECS.ActorQuerying.Queries.Default;

namespace Wallop.ECS.ActorQuerying.Parsing.Parslets.Default
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