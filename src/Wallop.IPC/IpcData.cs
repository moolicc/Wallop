using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC
{
    public readonly record struct IpcData
    {
        public IpcPacket Packet { get; init; }

        [System.Text.Json.Serialization.JsonConstructor]
        public IpcData(IpcPacket packet)
        {
            Packet = packet;
        }
    }
}
