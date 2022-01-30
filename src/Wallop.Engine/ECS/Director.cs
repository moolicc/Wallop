using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS
{
    public abstract class Director : IEcsMember
    {
        public string Name  { get; }

        protected Director()
            : this(string.Empty)
        { }

        protected Director(string name)
        {
            Name = name;
        }

        public virtual void Update() { }
        public virtual void Draw() { }
    }
}
