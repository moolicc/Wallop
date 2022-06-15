using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions;

namespace Wallop.Engine.ECS.ActorQuerying.Queries.Default
{
    public class FilterQuery : QueryBase
    {
        public IExpression FilterExpression { get; init; }

        public FilterQuery(IExpression filterExpression)
        {
            FilterExpression = filterExpression;
        }

        public override IEnumerable<IActor?> Evaluate(IEnumerable<IActor> workingSet, IEnumerable<IActor> originalSet)
        {
            return workingSet;
        }
    }
}
