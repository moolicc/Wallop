using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginPantry.Extending
{
    public class EntryPointContext
    {
        public Exposed Exposed { get; internal set; }
        public Type PluginType { get; internal set; }
        public object? PluginObject { get; internal set; }
        public string PluginId { get; internal set; }


        void RegisterEndpoint<TEndPointContext>(string handler)
        {
            RegisterEndpoint<TEndPointContext>(handler, PluginObject);
        }

        void RegisterEndpoint<TEndPointContext>(string handler, object? handlerInstance)
        {
            if(handlerInstance == null)
            {
                Exposed.SubscribeToStaticEndPoint<TEndPointContext>(handler, PluginType, PluginId);
            }
            else
            {
                Exposed.SubscribeToEndPoint<TEndPointContext>(handler, handlerInstance, PluginId);
            }
        }
    }
}
