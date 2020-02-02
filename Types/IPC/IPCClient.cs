using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.IPC
{
    public abstract class IPCClient
    {
        public abstract string[] GetOptionNames();

        public abstract bool CreateActiveConnection(string connectionName, (string, string) options);

        public abstract void DestroyConnection(string connectionName);

        public abstract bool SendData(string connectionName, object data);
    }
}
