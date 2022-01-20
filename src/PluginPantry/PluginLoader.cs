using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace PluginPantry
{
    public class PluginLoader
    {
        public Type PluginBaseType { get; set; }

        public PluginLoader()
            : this(typeof(object))
        {
        }

        public PluginLoader(Type pluginBaseType)
        {
            PluginBaseType = pluginBaseType;
        }

        public IEnumerable<PluginMetadata> LoadPlugins(string baseDir)
        {
            var files = Directory.GetFiles(baseDir, "*.dll", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                foreach(var pluginFound in LoadPluginAssembly(file))
                {
                    yield return pluginFound;
                }
            }
        }

        public IEnumerable<PluginMetadata> LoadPluginAssembly(string assemblyFile)
        {
            var assembly = Assembly.LoadFile(assemblyFile);
            foreach (var type in assembly.ExportedTypes)
            {
                if (!type.IsAssignableTo(PluginBaseType))
                {
                    // TODO: Message
                    continue;
                }

                foreach (var method in type.GetMethods())
                {
                    var attrib = method.GetCustomAttribute<Extending.PluginEntryPoint>();
                    if (attrib == null)
                    {
                        continue;
                    }

                    yield return new PluginMetadata(attrib.PluginName, attrib.PluginVersion, type, method, assemblyFile);
                }
            }
        }
    }
}
