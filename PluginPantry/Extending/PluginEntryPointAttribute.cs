using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginPantry.Extending
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PluginEntryPoint : Attribute
    {
        public string PluginName { get; private set; }
        public Version PluginVersion { get; private set; }

        public PluginEntryPoint(string pluginName, Version pluginVersion)
        {
            PluginName = pluginName;
            PluginVersion = pluginVersion;
        }
    }
}
