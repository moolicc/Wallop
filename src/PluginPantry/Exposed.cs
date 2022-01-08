using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginPantry
{
    public class Exposed
    {
        public PluginContext PluginContext { internal get; set; }

        public Exposed(PluginContext context)
        {
            PluginContext = context;
        }

        public void RegisterEndPoint<TEndPointContext, THandlerInstance>(string handler, string pluginId)
        {
            var instance = Activator.CreateInstance<THandlerInstance>();
            if(instance == null)
            {
                // TODO
                return;
            }
            RegisterEndPoint<TEndPointContext>(handler, instance, pluginId);
        }

        public void RegisterEndPoint<TEndPointContext>(Delegate handler, string pluginId)
        {
            if(handler.Target == null)
            {
                if(handler.Method.DeclaringType == null)
                {
                    //TODO
                    return;
                }
                RegisterStaticEndPoint<TEndPointContext>(handler.Method.Name, handler.Method.DeclaringType, pluginId);
            }
            else
            {
                RegisterEndPoint<TEndPointContext>(handler.Method.Name, handler.Target, pluginId);
            }
        }

        public void RegisterEndPoint<TEndPointContext>(string handler, object handlerInstance, string pluginId)
        {
            EndPointTable<TEndPointContext>.ForPluginContext(PluginContext).AddEndPoint(handler, handlerInstance.GetType(), handlerInstance, pluginId);
        }

        public void RegisterStaticEndPoint<TEndPointContext>(string handler, Type handlerType, string pluginId)
        {
            EndPointTable<TEndPointContext>.ForPluginContext(PluginContext).AddEndPoint(handler, handlerType, null, pluginId);
        }

        public void RegisterImplementation<TBase, TImplementation>() where TImplementation : TBase, new()
        {
            var instance = Activator.CreateInstance<TImplementation>();
            RegisterImplementation<TBase, TImplementation>(instance);
        }

        public void RegisterImplementation<TBase, TInstance>(TInstance instance) where TInstance : TBase
        {
            ImplementationTable<TBase>.ForPluginContext(PluginContext).AddInstance(instance);
        }
    }
}
