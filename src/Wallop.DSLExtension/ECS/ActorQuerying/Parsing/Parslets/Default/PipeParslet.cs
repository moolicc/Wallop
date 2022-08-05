using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.Parsing;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions.Default;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens;
using Wallop.Shared.ECS.ActorQuerying.Queries;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Parslets.Default
{
    public class PipeParslet : BinaryOperatorParsletBase
    {
        public PipeParslet()
            : base(false)
        {

        }

        protected override IExpression Parse(QueryParser parser, IToken token, IExpression lhs, IExpression rhs)
        {
            if (lhs is not IQuery && lhs is not QueryPipeExpression ||
                rhs is not IQuery && rhs is not QueryPipeExpression)
            {
                throw new InvalidOperationException("Expected Query collection operands.");
            }
            return new QueryPipeExpression(lhs, rhs);
        }
    }
}
