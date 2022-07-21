using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.ECS.ActorQuerying.Parsing.Expressions;

namespace Wallop.ECS.ActorQuerying.Queries
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
    }
}
