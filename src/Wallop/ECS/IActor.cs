using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.ECS
{
    public interface IActor : IEcsMember
    {
        public List<object> Components { get; }

    }
}
