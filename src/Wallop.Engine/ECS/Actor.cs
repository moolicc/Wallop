using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.ECS
{
    public class Actor : IActor
    {
        public List<object> Components { get; private set; }
        public string Name { get; private set; }

        public Actor()
            : this(string.Empty)
        {
        }

        public Actor(string name)
        {
            Components = new List<object>();
            Name = name;
        }

        public virtual void Update() { }
        public virtual void Draw() { }
    }
}
