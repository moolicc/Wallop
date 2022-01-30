using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying.Queries
{
    internal class AllQuery : IQuery
    {
        public IQuery Left { get; }

        public IQuery Right { get; }

        public IEnumerable<IActor> Evaluate(IEnumerable<IActor> workingSet, IEnumerable<IActor> originalSet)
        {
            return workingSet;
        }
    }
}
