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
                OnRemoveEntry?.Invoke(pluginId);
            }
        }
    }

    internal static class EndPointTable<T>
    {
        private static List<EndPointTableEntry> _endPoints;
        private static Type _endPointType;
        
        static EndPointTable()
        {
            _endPoints = new List<EndPointTableEntry>();
            _endPointType = typeof(T);

            EndPointTable.OnRemoveEntry += OnRemoveEntry;
        }

        private static void OnRemoveEntry(string pluginId)
        {
            foreach (var item in _endPoints)
            {
                if(item.PluginId == pluginId)
                {
                    _endPoints.Remove(item);
                    item.ExecutionTask?.Wait(500);
                }
            }
        }

        public static void AddEndPoint(string endPoint, object instance, string pluginId)
        {
            var instanceType = instance.GetType();

            foreach (var method in instanceType.GetMethods())
            {
                if(method.Name == endPoint)
                {
                    _endPoints.Add(new EndPointTableEntry(method, instance, pluginId, _endPointType));
                }
            }
        }

        public static void VisitEntries(Action<EndPointTableEntry> visitor)
        {
            foreach (var entry in _endPoints)
            {
                visitor(entry);
            }
        }
    }

    internal record EndPointTableEntry(MethodInfo Target, object Instance, string PluginId, Type EndPointType)
    {
        public Task? ExecutionTask { get; set; }
        public CancellationTokenSource? ExecutionTaskCancelToken { get; set; }

        public long ExecutionStartTime { get; set; }
        public long ExecutionEndTime { get; set; }

        public string Name => $"{EndPointType.Name}:{PluginId}:{Instance.GetType().Name}.{Target.Name}";
    }
}
