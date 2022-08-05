using Wallop.Shared.ECS.ActorQuerying.FilterMachine;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions.Default
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