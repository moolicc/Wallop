using Wallop.ECS.ActorQuerying.FilterMachine;

namespace Wallop.ECS.ActorQuerying.Parsing.Expressions.Default
{
    public class NotExpression : PrefixExpressionBase
    {
        public NotExpression(IExpression right) : base(right)
        {
        }

        public override void Evaluate(Machine machine)
        {
            Right.Evaluate(machine);

            var rhs = machine.PopStateValue<bool>(ValueKinds.Boolean);
            machine.PushState(new State(!rhs));
        }
    }
}