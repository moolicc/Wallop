using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginPantry
{
    public class PluginContext
    {
        public async Task ExecuteEndPoint<T>(T endPointInstance)
        {
            await EndPointRunner<T>.InvokeEndPointAsync(endPointInstance);
        }
    }
}
