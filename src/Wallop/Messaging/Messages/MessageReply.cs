using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Messaging.Messages
{
    public enum ReplyStatus
    {
        NotSpecified,
        Successful,
        Invalid,
        Failed,
    }

    public readonly record struct MessageReply(uint MessageId, ReplyStatus Status, string StatusMessage, Type? ContentType, object? Content);
}
