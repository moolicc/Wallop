using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.FilterMachine;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions.Default
{
    public class QueryPipeExpression : InfixExpressionBase
    {
        public QueryPipeExpression(IExpression left, IExpression right)
            : base(left, right)
        {
        }

        public override void Evaluate(Machine machine)
        {
            var left = Left as Queries.IQuery;
            var right = Right as Queries.IQuery;
            left?.Evaluate(machine);
            right?.Evaluate(machine);
        }
    }
}
