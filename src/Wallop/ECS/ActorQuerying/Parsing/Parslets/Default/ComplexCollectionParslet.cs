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
                return new FilterQuery(right);
            }
            else if(token is EditToken)
            {
                return new EditQuery(right);
            }
            throw new InvalidOperationException("Unexpected token for CollectionParslet.");
        }
    }
}
