using Wallop.ECS.ActorQuerying.FilterMachine;

namespace Wallop.ECS.ActorQuerying.Parsing.Expressions.Default
{
    public class PositivifyExpression : PrefixExpressionBase
    {
        public PositivifyExpression(IExpression right) : base(right)
        {
        }

        public override void Evaluate(Machine machine)
        {
            Right.Evaluate(machine);

            var state = machine.PopState();
            if(state.ValueType == ValueKinds.Integer)
            {
                machine.PushState(new State(+state.ValueI));
            }
            else if(state.ValueType == ValueKinds.Float)
            {
                machine.PushState(new State(+state.ValueD));
            }
            else
            {
                throw new InvalidOperationException("Unexpected token reached.");
            }
        }
    }
}