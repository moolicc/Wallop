using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.Queries;

namespace Wallop.Engine.ECS.ActorQuerying
{
    // TODO: Make queries and their associated parsing mechanisms extensible.
    // TODO: Create collection to operate on groups of components/actors.
    // TODO: Unify exception messages.

    internal static class QueryRunner
    {
        public static readonly IQuery AllQuery;

        public static bool UseCache { get; set; }


        private static Dictionary<string, IQuery> _queryCache;

        static QueryRunner()
        {
            _queryCache = new Dictionary<string, IQuery>();
            AllQuery = new Queries.Default.AllQuery();
        }

        public static void ClearCache()
            => _queryCache.Clear();

        public static int CacheSize()
            => _queryCache.Count;

        public static IEnumerable<IActor> RunQuery(string queryString, IEnumerable<IActor> workingSet, bool allowCache = true)
            => RunQuery(CreateQuery(queryString, allowCache), workingSet);

        public static IEnumerable<IActor> RunQuery(IQuery query, IEnumerable<IActor> workingSet)
        {
            var filterMachine = new FilterMachine.Machine(workingSet.ToList());
            query.Evaluate(filterMachine);
            return filterMachine.ActorSet;
        }

        public static IQuery CreateQuery(string queryString, bool allowCache = true)
        {
            IQuery result;
            if(allowCache && UseCache)
            {
                if(!_queryCache.TryGetValue(queryString, out result))
                {
                    result = new Parsing.QueryParser(queryString).Parse();
                    _queryCache.Add(queryString, result);
                }
            }
            else
            {
                result = new Parsing.QueryParser(queryString).Parse();
            }

            return result;            
        }
    }
}
