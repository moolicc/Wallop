using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.Queries;
using Wallop.Shared.ECS.ActorQuerying.Parsing;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Parslets
{
    public abstract class BinaryOperatorParsletBase : IInfixParslet
    {
        public int BindingPower { get; init; }
        public bool RightAssociative { get; init; }

        protected BinaryOperatorParsletBase(bool rightAssociative = false)
        {
            RightAssociative = rightAssociative;
            BindingPower = BindingPowerLookup.Get(GetType());
        }

        public virtual IExpression Parse(QueryParser parser, IToken token, IExpression lhs)
        {
            var rhs = parser.ParseNextExpression(BindingPower - (RightAssociative ? 1 : 0));
            return Parse(parser, token, lhs, rhs);
        }

        protected abstract IExpression Parse(QueryParser parser, IToken token, IExpression lhs, IExpression rhs);
    }
}
