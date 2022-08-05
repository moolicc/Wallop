using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.ECS.ActorQuerying.Queries.Default
{
    public class AllQuery : QueryBase
    {
        public override IEnumerable<IActor?> Evaluate(IEnumerable<IActor> workingSet, IEnumerable<IActor> originalSet)
        {
            return workingSet;
        }
    }
}
