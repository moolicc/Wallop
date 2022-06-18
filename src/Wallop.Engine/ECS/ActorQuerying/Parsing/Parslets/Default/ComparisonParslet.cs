﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions.Default;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens.Default;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Parslets.Default
{
    public class ComparisonParslet : BinaryOperatorParsletBase
    {
        protected override IExpression Parse(QueryParser parser, IToken token, IExpression lhs, IExpression rhs)
        {
            if(token is ComparisonToken compToken)
            {
                return new ComparisonExpression(lhs, rhs, compToken.Operator);
            }
            throw new InvalidOperationException("Unexpected token.");
        }
    }
}