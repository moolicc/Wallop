using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Messaging.Remoting
{
    internal readonly record struct PushMessage(MessageDirection Direction, Json.JsonMessage? PutMessage, Type? TakeType, uint? PreferredId);
}
