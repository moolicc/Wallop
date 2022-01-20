using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying
{
    internal static class QueryRunner
    {
        public static IEnumerable<Actor> RunQuery(Queries.IQuery query, IEnumerable<Actor> workingSet)
        {
            var original = workingSet.ToArray();
            return query.Evaluate(workingSet, original);
        }

        public static Queries.IQuery Parse(string queryString)
        {
            return new Queries.AllQuery();
        }
    }
}
