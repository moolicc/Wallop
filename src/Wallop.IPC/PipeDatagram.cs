using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC
{
    public enum PipeCommand
    {
        Enqueue,
        Dequeue,
        DequeueResponse,
        Disconnect,
    }

    public readonly record struct PipeDatagram(PipeCommand Command, string? IpcData);
}
