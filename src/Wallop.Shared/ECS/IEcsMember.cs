using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.ECS
{
    public interface IEcsMember
    {
        public string Name { get; }
        void Update();
        void Draw();
    }
}
