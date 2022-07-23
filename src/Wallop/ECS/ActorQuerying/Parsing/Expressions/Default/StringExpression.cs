﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.ECS.ActorQuerying.FilterMachine;

namespace Wallop.ECS.ActorQuerying.Parsing.Expressions.Default
{
    public class StringExpression : IExpression
    {
        public string Value { get; init; }

        public StringExpression(string value)
        {
            Value = value;
        }

        public void Evaluate(Machine machine)
        {
            machine.PushState(new State(Value));
        }
    }
}