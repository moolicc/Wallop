using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.FilterMachine;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions.Default
{
    public class AddExpression : InfixExpressionBase
    {
        public AddExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        public override void Evaluate(Machine machine)
        {
            Left.Evaluate(machine);
            Right.Evaluate(machine);
            var rhs = machine.PopStateValue();
            var lhs = machine.PopStateValue();

            if (lhs is string lhsS && rhs is string rhsS)
            {
                machine.PushState(new State(string.Concat(lhsS, rhsS)));
            }

            // Int on left-hand
            else if (lhs is int lhsI && rhs is int rhsI)
            {
                machine.PushState(new State(lhsI + rhsI));
            }
            else if (lhs is int lhsIWithF && rhs is float rhsF2)
            {
                machine.PushState(new State(lhsIWithF + rhsF2));
            }
            else if (lhs is int lhsIWithD && rhs is double rhsD2)
            {
                machine.PushState(new State(lhsIWithD + rhsD2));
            }

            // Float on left-hand
            else if (lhs is float lhsF && rhs is float rhsF)
            {
                machine.PushState(new State(lhsF + rhsF));
            }
            else if (lhs is float lhsF2 && rhs is int rhsI2)
            {
                machine.PushState(new State(lhsF2 + rhsI2));
            }
            else if (lhs is float lhsF3 && rhs is double rhsD3)
            {
                machine.PushState(new State(lhsF3 + rhsD3));
            }

            // Double on left-hand
            else if (lhs is double lhsD && rhs is double rhsD)
            {
                machine.PushState(new State(lhsD + rhsD));
            }
            else if (lhs is double lhsD2 && rhs is int rhsI3)
            {
                machine.PushState(new State(lhsD2 + rhsI3));
            }
            else if (lhs is double lhsD3 && rhs is float rhsF3)
            {
                machine.PushState(new State(lhsD3 + rhsF3));
            }
        }
    }
}
