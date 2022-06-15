using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying.Queries.Default
{
    public class LastQuery : QueryBase
    {
        public override IEnumerable<IActor?> Evaluate(IEnumerable<IActor> workingSet, IEnumerable<IActor> originalSet)
        {
            yield return workingSet.LastOrDefault();
        }
    }
}
