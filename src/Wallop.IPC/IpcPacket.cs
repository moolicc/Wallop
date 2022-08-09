using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC
{
    public readonly record struct IpcPacket
    {
        public IpcMessage Message { get; init; } 
        public string SourceApplication { get; init; }
        public string TargetApplication { get; init; }


        [System.Text.Json.Serialization.JsonConstructor]
        public IpcPacket(IpcMessage message, string sourceApplication, string targetApplication)
        {
            Message = message;
            SourceApplication = sourceApplication;
            TargetApplication = targetApplication;
            TargetApplication = targetApplication;
        }
    }
}
