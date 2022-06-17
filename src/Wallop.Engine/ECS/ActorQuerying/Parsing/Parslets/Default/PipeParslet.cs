using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions.Default;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Parslets.Default
{
    public class PipeParslet : BinaryOperatorParsletBase
    {
        public PipeParslet()
            : base(false)
        {

        }

        protected override IExpression Parse(QueryParser parser, IToken token, IExpression lhs, IExpression rhs)
        {
            if(lhs is not ActorQuerying.Queries.IQuery || rhs is not ActorQuerying.Queries.IQuery)
            {
                throw new InvalidOperationException("Expected Query collection operands.");
            }
            return new Expressions.Default.QueryPipeExpression(lhs, rhs);
        }
    }
}
