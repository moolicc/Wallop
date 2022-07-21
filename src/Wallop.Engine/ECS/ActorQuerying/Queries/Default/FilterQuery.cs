using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.ECS.ActorQuerying.FilterMachine;
using Wallop.ECS.ActorQuerying.Parsing.Expressions;

namespace Wallop.ECS.ActorQuerying.Queries.Default
{
    public class FilterQuery : ExecutableQuery
    {
        public IExpression FilterExpression { get; init; }

        public FilterQuery(IExpression filterExpression)
        {
            FilterExpression = filterExpression;
        }

        public override void Evaluate(Machine machine)
        {
            for (int i = 0; i < machine.ActorSet.Count; i++)
            {
                IActor? actor = machine.ActorSet[i];

                AddActorContextToMachine(machine, actor, true);
                FilterExpression.Evaluate(machine);
                RemoveActorContextFromMachine(machine, actor, true);

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
