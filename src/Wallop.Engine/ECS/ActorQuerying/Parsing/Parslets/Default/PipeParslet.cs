using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.ECS.ActorQuerying.Parsing.Expressions.Default;
using Wallop.ECS.ActorQuerying.Parsing.Tokens;
using Wallop.ECS.ActorQuerying.Queries;

namespace Wallop.ECS.ActorQuerying.Parsing.Parslets.Default
{
    public class PipeParslet : BinaryOperatorParsletBase
    {
        public PipeParslet()
            : base(false)
        {

        }

        protected override IExpression Parse(QueryParser parser, IToken token, IExpression lhs, IExpression rhs)
        {
            if((lhs is not IQuery && lhs is not QueryPipeExpression) ||
                (rhs is not IQuery && rhs is not QueryPipeExpression))
            {
                throw new InvalidOperationException("Expected Query collection operands.");
            }
            return new QueryPipeExpression(lhs, rhs);
        }
    }
}
