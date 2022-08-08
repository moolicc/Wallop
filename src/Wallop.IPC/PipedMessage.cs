using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC
{
    internal enum PipedMessageTypes
    {
        QueueRequest,
        DequeueRequest,
        Release,
    }

    internal readonly record struct PipedMessage(PipedMessageTypes Type, IpcData? Message)
    {

    }
}
