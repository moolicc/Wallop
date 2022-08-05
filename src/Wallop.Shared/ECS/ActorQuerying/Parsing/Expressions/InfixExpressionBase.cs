using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.Queries;
using Wallop.Shared.ECS.ActorQuerying.FilterMachine;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions
{
    public abstract class InfixExpressionBase : IExpression
    {
        public IExpression Left { get; init; }
        public IExpression Right { get; init; }

        public InfixExpressionBase(IExpression left, IExpression right)
        {
            Left = left;
            Right = right;
        }

        public abstract void Evaluate(Machine machine);
    }
}
