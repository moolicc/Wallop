﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.Queries;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions
{
    public abstract class PostfixExpressionBase : IExpression
    {
        public IExpression Left { get; init; }

        protected PostfixExpressionBase(IExpression left)
        {
            Left = left;
        }

        public abstract void Evaluate(FilterMachine.Machine machine);
    }
}