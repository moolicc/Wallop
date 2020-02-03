using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

namespace Wallop.Types.Loading
{
    public class TypeLoader
    {
        private Dictionary<(Type Type, string Library), object> _instanceCache;

        public TypeLoader()
        {
            _instanceCache = new Dictionary<(Type, string), object>();
        }

        public void ClearInstanceCache()
        {
            _instanceCache.Clear();
        }

        public void RemoveFromCache<T>(string library)
        {
            var key = (typeof(T), library);
            if (_instanceCache.ContainsKey(key))
            {
                _instanceCache.Remove(key);
            }
        }

        public T Load<T>(string library)
        {
            if(_instanceCache.TryGetValue((Type: typeof (T), Library: library), out var instance))
            {
                return (T)instance;
            }
            return LoadFromLibrary<T>(library, true);
        }

        public bool LoadFromCache<T>(out T value)
        {
            var tType = typeof(T);
            foreach (var item in _instanceCache)
            {
                if(item.Key.Type == tType)
                {
                    value = (T)item.Value;
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        public T LoadFromLibrary<T>(string library, bool cacheInstance)
        {
            var assembly = Assembly.LoadFile(library);
            var types = assembly.GetTypes();
            Type targetType = typeof(T);
            Type implementorType = null;

            foreach (var item in types)
            {
                if(item.IsAssignableFrom(targetType))
                {
                    implementorType = item;
                    break;
                }
            }

            if(implementorType == null)
            {
                return default(T);
            }

            T value = (T)Activator.CreateInstance(implementorType);
            var cacheKey = (Type: typeof(T), Library: library);
            if (cacheInstance)
            {
                _instanceCache.Add(cacheKey, value);
            }

            return value;
        }
    }
}
