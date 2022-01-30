﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS
{
    public class Actor : IActor
    {
        public List<Component> Components { get; private set; }
        public string Name { get; private set; }

        public Actor()
            : this(string.Empty)
        {
        }

        public Actor(string name)
        {
            Components = new List<Component>();
            Name = name;
        }


        public virtual void Update() { }
        public virtual void Draw() { }
    }
}
