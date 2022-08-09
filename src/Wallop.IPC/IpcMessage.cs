using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.IPC.Serialization;

namespace Wallop.IPC
{
    [Flags]
    public enum MessageTypes
    {
        Request,
        Response,
        Statement
    }

    // [System.Text.Json.Serialization.JsonConstructor]
    public readonly record struct IpcMessage
    {
        public MessageTypes Type { get; init; }
        public int MessageId { get; init; }
        public IntermediateValue Content { get; init; }
        public int? ReplyToId { get; init; }


        [System.Text.Json.Serialization.JsonConstructor]
        public IpcMessage(MessageTypes type, int messageId, IntermediateValue content, int? replyToId)
        {
            Type = type;
            MessageId = messageId;
            Content = content;
            ReplyToId = replyToId;
        }
    }
}
