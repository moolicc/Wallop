using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Scheduling
{
    public readonly record struct ActionRun(Action<object?> Action, object? State);
}
