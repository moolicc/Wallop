using Wallop.Shared.ECS.ActorQuerying.FilterMachine;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions.Default
{
    public class AndExpression : InfixExpressionBase
    {
        public AndExpression(IExpression left, IExpression right) : base(left, right)
        {
        }

        public override void Evaluate(Machine machine)
        {
            Left.Evaluate(machine);
            Right.Evaluate(machine);

            var rhs = machine.PopStateValue<bool>(ValueKinds.Boolean);
            var lhs = machine.PopStateValue<bool>(ValueKinds.Boolean);

            machine.PushState(new State(rhs && lhs));
        }
    }
}