using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions;

namespace Wallop.Engine.ECS.ActorQuerying.Queries
{
    /*
     * We want to express actors with certain names
     * Actors with certain components
     * 
     * Queries:
     *   *
     *     Returns all current actors
     *   
     *   
     */

    public interface IQuery : IExpression
    {
        public IEnumerable<IActor?> Evaluate(IEnumerable<IActor> workingSet, IEnumerable<IActor> originalSet);
    }
}
