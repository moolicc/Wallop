using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

namespace Wallop.Types.Loading
{
    public enum DefaultImplementedLibraries
    {
        IPC,

    }

    public class TypeLoader
    {
        public T LoadFromLibrary<T>(DefaultImplementedLibraries library)
        {
            return library switch
            {
                DefaultImplementedLibraries.IPC => LoadFromLibrary<T>(Defaults.IPCImplementation),
                _ => default(T),
            };
        }

        public T LoadFromLibrary<T>(string library)
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

            return (T)Activator.CreateInstance(implementorType);
        }
    }
}
