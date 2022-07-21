using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.ECS.ActorQuerying.FilterMachine;

namespace Wallop.ECS.ActorQuerying.Parsing.Expressions.Default
{
    public class BoolExpression : IExpression
    {
        public bool Value { get; init; }

        public BoolExpression(bool value)
        {
            Value = value;
        }

        public void Evaluate(Machine machine)
        {
            machine.PushState(new State(Value));
        }
    }
}
