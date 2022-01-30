using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying.Queries
{
    public interface IQuery
    {
        public IQuery Left { get; }
        public IQuery Right { get; }

        public IEnumerable<IActor> Evaluate(IEnumerable<IActor> workingSet, IEnumerable<IActor> originalSet);
    }
}
