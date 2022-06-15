﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions.Default;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Parslets.Default
{
    public class PowParslet : BinaryOperatorParsletBase
    {
        public PowParslet()
            : base(true)
        {

        }

        protected override IExpression Parse(QueryParser parser, IToken token, IExpression lhs, IExpression rhs)
        {
            if (token is Tokens.Default.PowToken aToken)
            {
                return new PowExpression(lhs, rhs);
            }

            throw new InvalidOperationException("Unknown pow token reached.");
        }
    }
}
