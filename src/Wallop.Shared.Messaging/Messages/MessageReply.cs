using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Messaging.Messages
{
    public enum ReplyStatus
    {
        NotSpecified,
        Successful,
        Invalid,
        Failed,
    }

    public readonly record struct MessageReply(uint MessageId, ReplyStatus Status, string StatusMessage, string? ContentType, object? Content);
}
