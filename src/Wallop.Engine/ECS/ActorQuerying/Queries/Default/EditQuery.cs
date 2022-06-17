using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.FilterMachine;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions;

namespace Wallop.Engine.ECS.ActorQuerying.Queries.Default
{
    public class EditQuery : IQuery
    {
        public IExpression EditExpression { get; init; }

        public EditQuery(IExpression editExpression)
        {
            EditExpression = editExpression;
        }

        public void Evaluate(Machine machine)
        {
            foreach (var actor in machine.ActorSet)
            {
                // TODO: Expanding object support. This way we can do: actor.PositionComponent.X = 100.
                // TODO: Implicit object name. This way we can do: PositionComponent.X = 100.
                machine.AddObjectMembers(actor, "actor");
                EditExpression.Evaluate(machine);
                machine.RemoveObjectMembers("actor");
            }
        }
    }
}
