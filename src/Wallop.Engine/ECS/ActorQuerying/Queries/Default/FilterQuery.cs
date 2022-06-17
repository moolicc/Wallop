using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.FilterMachine;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions;

namespace Wallop.Engine.ECS.ActorQuerying.Queries.Default
{
    public class FilterQuery : IQuery
    {
        public IExpression FilterExpression { get; init; }

        public FilterQuery(IExpression filterExpression)
        {
            FilterExpression = filterExpression;
        }

        public void Evaluate(Machine machine)
        {
            for (int i = 0; i < machine.ActorSet.Count; i++)
            {
                // TODO: Expanding object support. This way we can do: actor.PositionComponent.X = 100.
                // TODO: Implicit object name. This way we can do: PositionComponent.X = 100.

                IActor? actor = machine.ActorSet[i];

                machine.AddObjectMembers(actor, "actor");
                FilterExpression.Evaluate(machine);
                machine.RemoveObjectMembers("actor");

                var filterResult = machine.PopState(ValueKinds.Boolean);
                if(!filterResult.ValueB)
                {
                    machine.ActorSet.RemoveAt(i);
                    i--;
                }
            }

        }
    }
}
