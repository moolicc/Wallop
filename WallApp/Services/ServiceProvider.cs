using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.Services
{
    static class ServiceProvider
    {
        private static Dictionary<string, object> _services;

        static ServiceProvider()
        {
            _services = new Dictionary<string, object>();
        }

        public static void Init()
        {
            LocateServices();
            ResolveServiceReferences();
        }


        public static void Provide<T>(T service)
        {
            Provide(typeof(T).Name, service);
        }
        public static void Provide<T>(string key, T service)
        {
            _services.Add(BuildKeyWithBaseKey(typeof(T), key), service);
        }

        public static void SetService<T>(T service)
        {
            SetService(typeof(T).Name, service);
        }
        public static void SetService<T>(string key, T service)
        {
            var realKey = BuildKeyWithBaseKey(typeof(T), key);
            if (!_services.ContainsKey(realKey))
            {
                _services.Add(realKey, service);
            }
            else
            {
                _services[realKey] = service;
            }
        }

        public static T GetService<T>()
        {
            return GetService<T>(typeof(T).Name);
        }
        public static T GetService<T>(string key)
        {
            var realKey = BuildKeyWithBaseKey(typeof(T), key);

            if(!_services.TryGetValue(realKey, out var result))
            {
                ThrowKeyNotfound(typeof(T), key);
            }

            return (T)result;
        }

        public static void KillReferences()
        {
            _services.Clear();
            _services = null;
        }

        private static void LocateServices()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var serviceTypes = assembly.GetTypes().Where(t => t.GetCustomAttributes<ServiceAttribute>().Any());/*
                {
                    var attribs = t.GetCustomAttributes(true);
                    return attribs.Any(a => a.GetType() == typeof(ServiceAttribute));
                });*/
            foreach (var serviceType in serviceTypes.ToArray())
            {
                if (CreateInstance(serviceType, out var instance))
                {
                    var attrib = serviceType.GetCustomAttribute<ServiceAttribute>();
                    _services.Add(BuildKeyWithAttribute(serviceType, attrib), instance);
                }
            }
        }

        private static void ResolveServiceReferences()
        {
            foreach (var item in _services)
            {
                var properties = item.Value.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => p.GetCustomAttribute<ServiceReferenceAttribute>() != null);
                var fields = item.Value.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => p.GetCustomAttribute<ServiceReferenceAttribute>() != null);

                AssignPropertyServiceReferences(item.Value, properties);
                AssignFieldServiceReferences(item.Value, fields);

            }
        }

        private static void AssignPropertyServiceReferences(object instance, IEnumerable<PropertyInfo> properties)
        {
            foreach (var item in properties)
            {
                var attrib = item.GetCustomAttribute<ServiceReferenceAttribute>();
                var service = GetServiceForReference(attrib, item.Name, item.PropertyType, instance.GetType());
                item.SetValue(instance, service);
            }
        }
        private static void AssignFieldServiceReferences(object instance, IEnumerable<FieldInfo> fields)
        {
            foreach (var item in fields)
            {
                var attrib = item.GetCustomAttribute<ServiceReferenceAttribute>();
                var service = GetServiceForReference(attrib, item.Name, item.FieldType, instance.GetType());
                item.SetValue(instance, service);
            }
        }
        private static object GetServiceForReference(ServiceReferenceAttribute attribute, string memberName, Type serviceType, Type referenceOwner)
        {
            var key = BuildKeyWithAttribute(serviceType, attribute);

            if (!_services.TryGetValue(key, out var service))
            {
                string message = $"Referenced service with key '{key}' was not found.\r\nReference: {referenceOwner}/{memberName}";
                var possibility = _services.FirstOrDefault(i => i.Value.GetType() == serviceType);
                if (possibility.Value != null)
                {
                    message += $"\r\nDid you mean to use key '{possibility.Key}'?";
                }
                throw new KeyNotFoundException(message);
            }
            return service;
        }


        private static bool CreateInstance(Type type, out object result)
        {
            if (type.IsInterface || type.IsAbstract)
            {
                result = null;
                return false;
            }

            result = Activator.CreateInstance(type);
            return true;
        }

        private static string GetKey(Type type, string originalKey)
        {
            if(originalKey.StartsWith("/"))
            {
                return $"{type.Name}{originalKey}";
            }
            return originalKey;
        }
        private static string BuildKeyWithAttribute(Type type, Attribute attribute)
        {
            string key = null;
             
            if(attribute is ServiceAttribute serviceAttrib)
            {
                key = serviceAttrib.Key;
            }
            else if(attribute is ServiceReferenceAttribute refAttrib)
            {
                key = refAttrib.Key;
            }
            if(key.IsNull())
            {
                return type.Name;
            }

            return GetKey(type, key);
        }
        private static string BuildKeyWithBaseKey(Type targetType, string providedKey)
        {
            return GetKey(targetType, providedKey);
        }

        private static void ThrowKeyNotfound(Type targetType, string key)
        {
            string message = $"Service with key '{key}' was not found.";
            var possibility = _services.FirstOrDefault(i => i.Value.GetType() == targetType);
            if (possibility.Value != null)
            {
                message += $"\r\nDid you mean to use key '{possibility.Key}'?";
            }
            throw new KeyNotFoundException(message);
        }
    }
}
