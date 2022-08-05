using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.ECS
{
    public interface IActor : IEcsMember
    {
        List<object> Components { get; }
    }
}
