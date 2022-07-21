using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.ECS.ActorQuerying.Queries;

namespace Wallop.ECS.ActorQuerying.Parsing.Expressions
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

        public abstract void Evaluate(FilterMachine.Machine machine);
    }
}
