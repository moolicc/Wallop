using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC.Serialization
{
    public interface ISerializer
    {
        string Serialize(object data);
        T Deserialize<T>(string data);
        object Deserialize(string data);
    }
}
