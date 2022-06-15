using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens.Default;
using Wallop.Engine.ECS.ActorQuerying.Queries;
using Wallop.Engine.ECS.ActorQuerying.Queries.Default;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Parslets.Default
{
    public class ComplexCollectionParslet : PrefixParsletBase
    {
        public ComplexCollectionParslet()
            : base(false)
        {
        }

        protected override IExpression ParseStandalone(QueryParser parser, IToken token)
        {
            throw new NotImplementedException();
        }

        protected override IExpression Parse(QueryParser parser, IExpression right, IToken token)
        {
            if (token is FilterToken)
            {
                parser.Consume<PipeToken>();
                return new FilterQuery(right);
            }
            throw new InvalidOperationException("Unexpected token for CollectionParslet.");
        }
    }
}
