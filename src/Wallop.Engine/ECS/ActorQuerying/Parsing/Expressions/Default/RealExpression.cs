using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.FilterMachine;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions.Default
{
    internal class RealExpression : IExpression
    {
        public double Value { get; init; }

        public RealExpression(double value)
        {
            Value = value;
        }

        public void Evaluate(Machine machine)
        {
            machine.PushState(new State(Value));
        }
    }
}
