using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Messaging.Messages
{
    public readonly record struct SetActiveLayoutMessage(string LayoutName);
}
