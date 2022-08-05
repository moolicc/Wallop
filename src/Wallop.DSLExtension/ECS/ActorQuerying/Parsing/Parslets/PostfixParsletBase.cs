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
    public abstract class PostfixParsletBase : IInfixParslet
    {
        public int BindingPower { get; init; }

        protected PostfixParsletBase()
        {
            BindingPower = BindingPowerLookup.Get(GetType());
        }

        public abstract IExpression Parse(QueryParser parser, IToken token, IExpression lhs);
    }
}
