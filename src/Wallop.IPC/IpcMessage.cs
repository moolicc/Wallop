using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC
{
    [Flags]
    public enum MessageTypes
    {
        Request,
        Response,
        Statement
    }

    public readonly record struct IpcMessage(MessageTypes Type, int MessageId, string Message, int? ReplyToId);
}
