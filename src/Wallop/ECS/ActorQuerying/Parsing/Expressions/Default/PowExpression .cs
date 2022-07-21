using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.ECS.ActorQuerying.FilterMachine;

namespace Wallop.ECS.ActorQuerying.Parsing.Expressions.Default
{
    public class PowExpression : InfixExpressionBase
    {
        public PowExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        public override void Evaluate(Machine machine)
        {
            Left.Evaluate(machine);
            Right.Evaluate(machine);
            var rhs = machine.PopStateValue();
            var lhs = machine.PopStateValue();

            if(lhs is int lhsI && rhs is int rhsI)
            {
                machine.PushState(new State((int)Math.Pow(lhsI, rhsI)));
            }
            else if (lhs is float lhsF && rhs is float rhsF)
            {
                machine.PushState(new State((float)Math.Pow(lhsF, rhsF)));
            }
            else if (lhs is double lhsD && rhs is double rhsD)
            {
                machine.PushState(new State(Math.Pow(lhsD, rhsD)));
            }
        }
    }
}
