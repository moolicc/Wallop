using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.FilterMachine;

namespace Wallop.Shared.ECS.ActorQuerying.Queries
{
    public abstract class ExecutableQuery : IQuery
    {
        protected void AddActorContextToMachine(Machine machine, IActor actor, bool expand)
        {
            machine.AddObjectMembers(actor, expand, "actor");

            if (!expand)
            {
                return;
            }

            foreach (var component in actor.Components)
            {
                machine.AddObjectMembers(component, false, component.GetType().ToString());
            }
        }

        protected void RemoveActorContextFromMachine(Machine machine, IActor actor, bool expand)
        {
            machine.RemoveObjectMembers("actor");

            if (!expand)
            {
                return;
            }

            foreach (var component in actor.Components)
            {
                machine.RemoveObjectMembers(component.GetType().ToString());
            }
        }

        public abstract void Evaluate(Machine machine);
    }
}
