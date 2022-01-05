using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginPantry.Extending
{
    public interface IEntryPointContext
    {
        void RegisterEndpoint<T>(string handler);
        void RegisterEndpoint<T>(string handler, object handlerInstance);
    }
}
