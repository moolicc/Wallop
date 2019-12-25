using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WallApp.App.Services
{
    static class ServiceLocator
    {
        private static Dictionary<Type, object> _services;

        static ServiceLocator()
        {
            _services = new Dictionary<Type, object>();
        }

        public static void LocateAllServices()
        {
            var assembly = Assembly.GetExecutingAssembly();

            List<Type> serviceTypes = new List<Type>();
            foreach (var item in assembly.GetTypes())
            {
                if (item != typeof(IService) && typeof(IService).IsAssignableFrom(item))
                {
                    serviceTypes.Add(item);
                }
            }
            var services = new List<IService>(serviceTypes.Count());

            Console.WriteLine($"Instantiating {services.Count} services...");

            foreach (var type in serviceTypes)
            {
                Console.Write($"  {type.Name}...");
                object instance = Activator.CreateInstance(type);
                Console.WriteLine($"OK");

                RegisterService(type, instance);
                services.Add((IService)instance);
            }

            Console.WriteLine($"Initializing {services.Count} services...");
            services.Sort(new Comparison<IService>((x, y) => Comparer<int>.Default.Compare(-x.InitPriority, -y.InitPriority)));
            foreach (var item in services)
            {
                Console.Write($"  {item.GetType().Name}...");
                item.Initialize();
                Console.WriteLine($"OK");
            }
        }

        public static void RegisterService(Type serviceType, object instance)
        {
            _services.Add(serviceType, instance);
        }

        public static void RegisterService<T>(T service)
        {
            _services.Add(typeof(T), service);
        }

        public static T Locate<T>()
        {
            return (T)_services[typeof(T)];
        }
    }
}
