﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.FilterMachine;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions.Default
{
    public class DivideExpression : InfixExpressionBase
    {
        public DivideExpression(IExpression left, IExpression right) : base(left, right)
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
                machine.PushState(new State(lhsI / rhsI));
            }
            else if (lhs is float lhsF && rhs is float rhsF)
            {
                machine.PushState(new State(lhsF / rhsF));
            }
            else if (lhs is double lhsD && rhs is double rhsD)
            {
                machine.PushState(new State(lhsD / rhsD));
            }
        }
    }
}
