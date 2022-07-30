using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC
{
    public interface IIpcEndpoint
    {
        bool IsConnected { get; }
        string InstanceName { get; }

        void Lock();
        void Write(byte[] buffer, int offset, int count);
        void Read(byte[] buffer, int offset, int count);
        void Release();
    }
}
