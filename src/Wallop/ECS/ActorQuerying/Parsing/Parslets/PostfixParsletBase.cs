using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.ECS.ActorQuerying.Parsing.Tokens;
using Wallop.ECS.ActorQuerying.Queries;

namespace Wallop.ECS.ActorQuerying.Parsing.Parslets
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
