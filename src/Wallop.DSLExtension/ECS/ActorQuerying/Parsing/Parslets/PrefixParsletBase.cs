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
    public abstract class PrefixParsletBase : IPrefixParslet
    {
        public int BindingPower { get; init; }

        protected bool IsStandalone { get; init; }

        protected PrefixParsletBase(bool isStandalone = false)
        {
            IsStandalone = isStandalone;
            if (!isStandalone)
            {
                BindingPower = BindingPowerLookup.Get(GetType());
            }
        }

        public virtual IExpression Parse(QueryParser parser, IToken token)
        {
            if (IsStandalone)
            {
                return ParseStandalone(parser, token);
            }

            var right = parser.ParseNextExpression(BindingPower);
            return Parse(parser, right, token);
        }

        protected abstract IExpression ParseStandalone(QueryParser parser, IToken token);

        protected abstract IExpression Parse(QueryParser parser, IExpression right, IToken token);
    }
}
