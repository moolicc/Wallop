using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginPantry
{
    internal class DynamicImplementationTable<TBase>
    {
        private static readonly Dictionary<PluginContext, DynamicImplementationTable<TBase>> _instances;

        private PluginContext _context;
        // TODO: Entries need to carry plugin information.
        private List<Type> _implementationTypes;

        static DynamicImplementationTable()
        {
            _instances = new Dictionary<PluginContext, DynamicImplementationTable<TBase>>();
        }

        public static void ClearTable(PluginContext context)
        {
            _instances.Remove(context);
        }

        public static DynamicImplementationTable<TBase> ForPluginContext(PluginContext context)
        {
            if (!_instances.TryGetValue(context, out var table))
            {
                table = new DynamicImplementationTable<TBase>(context);
                _instances.Add(context, table);
            }
            return table;
        }

        private DynamicImplementationTable(PluginContext context)
        {
            _context = context;
            _implementationTypes = new List<Type>();
        }

        public void AddImplementationType<TSub>() where TSub : TBase
        {
            AddImplementationType(typeof(TSub));
        }

        public void AddImplementationType(Type type)
        {
            _implementationTypes.Add(type);
        }

        public void CreateInstances<TContext>(TContext context)
        {
            foreach (var implementationType in _implementationTypes)
            {
                var ctors = implementationType.GetConstructors();
                var argValues = new List<object?>();
                bool found = false;

                foreach (var ctor in ctors)
                {
                    found = Util.TryGetParamValues(ctor.GetParameters(), context, argValues);
                    if (found)
                    {
                        break;
                    }
                    argValues.Clear();
                }

                if(found)
                {
                    var instance = Activator.CreateInstance(implementationType, argValues.ToArray());
                    if(instance != null)
                    {
                        ImplementationTable<TBase>.ForPluginContext(_context).AddInstance((TBase)instance);
                    }
                }
                else
                {
                    // TODO: Bubble up message.
                }
            }
        }
    }
}
