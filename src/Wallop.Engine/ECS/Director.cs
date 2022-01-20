using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS
{
    public abstract class Director
    {
        public virtual void Update() { }
        public virtual void Draw() { }
    }
}
