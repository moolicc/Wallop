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
            if(Left is Queries.IQuery lhsQ)
            {
                lhsQ?.Evaluate(machine);
            }
            else if(Left is Expressions.Default.QueryPipeExpression lhsP)
            {
                lhsP?.Evaluate(machine);
            }
            else
            {
                throw new InvalidOperationException("Expected left-hand operand to be a pipe or query.");
            }

            if (Right is Queries.IQuery rhsQ)
            {
                rhsQ?.Evaluate(machine);
            }
            else if (Right is Expressions.Default.QueryPipeExpression rhsP)
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
