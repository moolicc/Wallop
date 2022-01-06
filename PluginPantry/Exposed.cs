using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginPantry
{
    public class Exposed
    {
        public PluginContext PluginContext { get; private set; }

        public Exposed(PluginContext context)
        {
            PluginContext = context;
        }

        public void SubscribeToEndPoint<TEndPointContext>(string handler, object handlerInstance, string pluginId)
        {
            EndPointTable<TEndPointContext>.ForPluginContext(PluginContext).AddEndPoint(handler, handlerInstance.GetType(), handlerInstance, pluginId);
        }

        public void SubscribeToStaticEndPoint<TEndPointContext>(string handler, Type handlerType, string pluginId)
        {
            EndPointTable<TEndPointContext>.ForPluginContext(PluginContext).AddEndPoint(handler, handlerType, null, pluginId);
        }
    }
}
