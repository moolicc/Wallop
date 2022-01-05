using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace PluginPantry.Loading
{
    public class PluginLoader
    {
        public IEnumerable<PluginMetadata> FindPlugins(string baseDir, Type baseType)
        {
            var files = Directory.GetFiles(baseDir, "*.dll", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var assembly = Assembly.Load(file);
                foreach(var type in assembly.ExportedTypes)
                {
                    if(!type.IsAssignableTo(baseType))
                    {
                        // TODO: Message
                        continue;
                    }

                    foreach (var method in type.GetMethods())
                    {
                        var attrib = method.GetCustomAttribute<Extending.PluginEntryPoint>();
                        if(attrib == null)
                        {
                            continue;
                        }

                        yield return new PluginMetadata(attrib.PluginName, attrib.PluginVersion, method, file);
                    }
                }
            }
        }

        public void ReloadPluginAssembly(string assembly)
        {
        }

        public void ReloadPlugin(PluginMetadata pluginMetadata)
        {
            // TODO: Cleanup plugin

            if (EndPointRunner.CancelTokens.TryGetValue(pluginMetadata.Id, out var cancelToken))
            {
                cancelToken.Cancel();
                cancelToken.Dispose();

            }
        }
    }
}
