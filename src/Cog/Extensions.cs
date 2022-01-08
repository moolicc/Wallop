using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cog
{
    internal static class Extensions
    {
        public static TValue WaitGet<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> instance, TKey key)
        {
            TValue value;
            while(!instance.TryGetValue(key, out value))
            {
                Thread.Sleep(50);
            }
            return value;
        }

        public static void WaitAdd<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> instance, TKey key, TValue value)
        {
            while (!instance.TryAdd(key, value))
            {
                Thread.Sleep(50);
            }
        }

        public static IEnumerable<KeyValuePair<string, object>> ToValues(this IEnumerable<KeyValuePair<string, SettingInfo>> instance)
        {
            return instance.Select(kvp => new KeyValuePair<string, object>(kvp.Key, kvp.Value.GetValue()));
        }

        public static bool Inherits<TBase>(this Type subType)
            => typeof(TBase).IsAssignableFrom(subType);
    }
}
