using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.FilterMachine;
using Wallop.Shared.ECS.ActorQuerying.Queries;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions.Default
{
    public class QueryPipeExpression : InfixExpressionBase
    {
        public QueryPipeExpression(IExpression left, IExpression right)
            : base(left, right)
        {
        }

        public override void Evaluate(Machine machine)
        {
            if (Left is IQuery lhsQ)
            {
                lhsQ?.Evaluate(machine);
            }
            else if (Left is QueryPipeExpression lhsP)
            {
                lhsP?.Evaluate(machine);
            }
            else
            {
                throw new InvalidOperationException("Expected left-hand operand to be a pipe or query.");
            }

            if (Right is IQuery rhsQ)
            {
                rhsQ?.Evaluate(machine);
            }
            else if (Right is QueryPipeExpression rhsP)
            {
                rhsP?.Evaluate(machine);
            }
            else
            {
                throw new InvalidOperationException("Expected right-hand operand to be a pipe or query.");
            }
        }
    }
}
