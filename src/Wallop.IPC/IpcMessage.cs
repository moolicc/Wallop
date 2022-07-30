using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC
{
    public readonly record struct IpcMessage(string Content, string SourceApplication, string TargetApplication);
}
