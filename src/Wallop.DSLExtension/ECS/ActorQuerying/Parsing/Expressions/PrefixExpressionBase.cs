using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.Queries;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions
{
    public abstract class PrefixExpressionBase : IExpression
    {
        public IExpression Right { get; init; }

        protected PrefixExpressionBase(IExpression right)
        {
            Right = right;
        }

        public abstract void Evaluate(FilterMachine.Machine machine);
    }
}
