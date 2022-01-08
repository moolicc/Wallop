using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginPantry
{
    internal class ImplementationTable<TBase>
    {
        private static readonly Dictionary<PluginContext, ImplementationTable<TBase>> _instances;

        private List<TBase> _implementations;

        static ImplementationTable()
        {
            _instances = new Dictionary<PluginContext,ImplementationTable<TBase>>();
        }

        public static void ClearTable(PluginContext context)
        {
            _instances.Remove(context);
        }

        public static ImplementationTable<TBase> ForPluginContext(PluginContext context)
        {
            if (!_instances.TryGetValue(context, out var table))
            {
                table = new ImplementationTable<TBase>();
                _instances.Add(context, table);
            }
            return table;
        }

        private ImplementationTable()
        {
            _implementations = new List<TBase>();
        }

        public void AddInstance(TBase instance)
        {
            _implementations.Add(instance);
        }

        public IEnumerable<TBase> GetInstances()
        {
            return _implementations.AsReadOnly();
        }
    }
}
