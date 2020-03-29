using System;
using System.Collections.Generic;
using System.Text;
using Wallop.Cmd;

namespace Wallop.IPC
{
    public class Client : Wallop.IPC.IPCClient
    {
        public override bool CreateActiveConnection(string connectionName, ParseResults args)
        {
            Console.WriteLine($">> Creating connection '{connectionName}' ...");
            return true;
        }

        public override void DestroyConnection(string connectionName)
        {
            throw new NotImplementedException();
        }

        public override Option[] GetOptions()
        {
            return Array.Empty<Option>();
        }

        public override bool SendData(string connectionName, object data)
        {
            return false;
        }
    }
}
