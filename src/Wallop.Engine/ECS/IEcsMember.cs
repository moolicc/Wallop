﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS
{
    public interface IEcsMember : IFrameInvocable
    {
        public string Name { get; }
    }
}