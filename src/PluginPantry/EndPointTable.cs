using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PluginPantry
{
    internal delegate void OnRemoveEntry(string pluginId);

    internal static class EndPointTable
    {
        public static event OnRemoveEntry? OnRemoveEntry;

        public static void RemovePlugin(string pluginId)
        {
            if (!string.IsNullOrEmpty(pluginId))
            {
                if(OnRemoveEntry != null)
                {
                    OnRemoveEntry(pluginId);
                }
            }
        }
    }

    internal class EndPointTable<TEndPointContext>
    {
        private static Dictionary<PluginContext, EndPointTable<TEndPointContext>> _instances;

        private List<EndPointTableEntry> _endPoints;
        private Type _endPointType;
       
        static EndPointTable()
        {
            _instances = new Dictionary<PluginContext, EndPointTable<TEndPointContext>>();
        }

        public static void ClearTable(PluginContext context)
        {
            _instances.Remove(context);
        }

        public static EndPointTable<TEndPointContext> ForPluginContext(PluginContext context)
        {
            if(!_instances.TryGetValue(context, out var endPointTable))
            {
                endPointTable = new EndPointTable<TEndPointContext>();
                _instances.Add(context, endPointTable);
            }
            return endPointTable;
        }

        private EndPointTable()
        {
            _endPoints = new List<EndPointTableEntry>();
            _endPointType = typeof(TEndPointContext);

            EndPointTable.OnRemoveEntry += OnRemoveEntry;
        }


        private void OnRemoveEntry(string pluginId)
        {
            for (int i = 0; i < _endPoints.Count; i++)
            {
                EndPointTableEntry? item = _endPoints[i];
                if (item.PluginId == pluginId)
                {
                    _endPoints.RemoveAt(i);
                    i--;
                }
            }
        }

        public void AddEndPoint(string endPoint, Type instanceType, object? instance, string pluginId)
        {
            foreach (var method in instanceType.GetMethods())
            {
                if(method.Name == endPoint)
                {
                    _endPoints.Add(new EndPointTableEntry(method, instanceType, instance, pluginId, _endPointType));
                }
            }
        }

        public void VisitEntries(Action<EndPointTableEntry> visitor)
        {
            foreach (var entry in _endPoints)
            {
                visitor(entry);
            }
        }

        public int GetEntryCount()
        {
            return _endPoints.Count;
        }
    }

    internal record EndPointTableEntry(MethodInfo Target, Type InstanceType, object? Instance, string PluginId, Type EndPointType)
    {
        public long ExecutionStartTime { get; set; }
        public long ExecutionEndTime { get; set; }

        public string Name => $"{EndPointType.Name}:{PluginId}:{InstanceType.Name}.{Target.Name}";
    }
}
