using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC
{
    public interface IIpcClient : IIpcEndpoint
    {
        void Connect(string instanceName);
    }
}
