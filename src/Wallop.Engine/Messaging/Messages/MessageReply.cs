using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Messaging.Messages
{
    public readonly record struct MessageReply(uint MessageId, Type? DataType, object? Data);
}
