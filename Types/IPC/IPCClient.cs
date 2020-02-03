using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.IPC
{
    public abstract class IPCClient
    {
        public abstract Cmd.Option[] GetOptions();

        public abstract bool CreateActiveConnection(string connectionName, Cmd.ParseResults args);

        public abstract void DestroyConnection(string connectionName);

        public abstract bool SendData(string connectionName, object data);
    }
}
