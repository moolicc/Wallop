using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cog
{
    internal static class ReflectionUtils
    {
        public static IEnumerable<Type> GetSettingsTypes()
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            if (assembly == null)
            {
                throw new InvalidOperationException("Failed to resolve assembly.");
            }

            foreach (var item in assembly.GetTypes())
            {
                if(item.IsAssignableTo(typeof(Settings)))
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<string> GetMemberNames(Type type)
        {
            foreach (var item in type.GetProperties())
            {
                yield return item.Name;
            }

            foreach (var item in type.GetFields())
            {
                yield return item.Name;
            }
        }

        public static void VisitMemberValues(Type type, object instance, Action<string, object> visitor)
        {
            foreach (var item in type.GetProperties())
            {
                visitor(item.Name, item.GetValue(instance));
            }

            foreach (var item in type.GetFields())
            {
                visitor(item.Name, item.GetValue(instance));
            }
        }

        public static void VisitMembers(Type type, object instance, Action<PropertyInfo?, FieldInfo?> visitor)
        {

            foreach (var item in type.GetProperties())
            {
                visitor(item, null);
            }

            foreach (var item in type.GetFields())
            {
                visitor(null, item);
            }
        }

        public static void VisitMembers(Type type, object instance, Func<PropertyInfo?, FieldInfo?, bool> visitor)
        {

            foreach (var item in type.GetProperties())
            {
                if(visitor(item, null))
                {
                    return;
                }
            }

            foreach (var item in type.GetFields())
            {
                if (visitor(null, item))
                {
                    return;
                }
            }
        }
    }
}
