using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.FilterMachine;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens.Default;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions.Default
{
    public class ComparisonExpression : InfixExpressionBase
    {
        ComparisonModes ComparisonMode { get; init; }

        public ComparisonExpression(IExpression left, IExpression right, ComparisonModes comparisonMode)
            : base(left, right)
        {
            ComparisonMode = comparisonMode;
        }

        public override void Evaluate(Machine machine)
        {
            Left.Evaluate(machine);
            Right.Evaluate(machine);

            var rhsComp = machine.PopStateValue() as IComparable;
            var lhsComp = machine.PopStateValue() as IComparable;

            if(lhsComp == null || rhsComp == null)
            {
                throw new InvalidOperationException();
            }

            var comparison = lhsComp.CompareTo(rhsComp);
            var result = ComparisonMode switch
            {
                ComparisonModes.Equal => comparison == 0,
                ComparisonModes.NotEqual => comparison != 0,
                ComparisonModes.Less => comparison < 0,
                ComparisonModes.LessOrEqual => comparison <= 0,
                ComparisonModes.GreaterOrEqual => comparison > 0,
                ComparisonModes.Greater => comparison >= 0,
                _ => throw new NotImplementedException(),
            };

            machine.PushState(new State(result));
        }
    }
}
