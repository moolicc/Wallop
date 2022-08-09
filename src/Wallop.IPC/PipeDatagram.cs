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

    public readonly record struct PipeDatagram
    {
        public PipeCommand Command { get; init; }
        public IpcData? IpcData { get; init; }

        public PipeDatagram(PipeCommand command, IpcData? ipcData)
        {
            Command = command;
            IpcData = ipcData;
        }
    }
}
