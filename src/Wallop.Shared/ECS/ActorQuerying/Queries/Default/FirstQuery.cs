using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.Queries;

namespace Wallop.Shared.ECS.ActorQuerying.Queries.Default
{
    public class FirstQuery : QueryBase
    {
        public override IEnumerable<IActor?> Evaluate(IEnumerable<IActor> workingSet, IEnumerable<IActor> originalSet)
        {
            yield return workingSet.FirstOrDefault();
        }
    }
}
