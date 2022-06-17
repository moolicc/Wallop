using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying.Queries
{
    public abstract class QueryBase : IQuery
    {
        public abstract IEnumerable<IActor?> Evaluate(IEnumerable<IActor> workingSet, IEnumerable<IActor> originalSet);

        public void Evaluate(FilterMachine.Machine machine)
        {
            Evaluate(machine.ActorSet, machine.OriginalActorSet);
        }
    }
}
