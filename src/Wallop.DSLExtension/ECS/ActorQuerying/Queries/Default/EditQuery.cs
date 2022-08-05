using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.FilterMachine;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions;

namespace Wallop.Shared.ECS.ActorQuerying.Queries.Default
{
    public class EditQuery : ExecutableQuery
    {
        public IExpression EditExpression { get; init; }

        public EditQuery(IExpression editExpression)
        {
            EditExpression = editExpression;
        }

        public override void Evaluate(Machine machine)
        {
            foreach (var actor in machine.ActorSet)
            {
                AddActorContextToMachine(machine, actor, true);
                EditExpression.Evaluate(machine);
                RemoveActorContextFromMachine(machine, actor, true);
            }
        }
    }
}
