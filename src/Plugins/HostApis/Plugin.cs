using PluginPantry.Extending;
using Wallop.DSLExtension.Scripting;
using Wallop.DSLExtension.Types.Plugin;

namespace HostApis
{
    public class Plugin
    {
        [PluginEntryPoint("Host APIs", "1.0.0.0")]
        public void Startup(PluginInformation pluginInfo)
        {
            pluginInfo.Exposed.RegisterImplementation<IHostApi, TrippyGLApi>();
            pluginInfo.Exposed.RegisterImplementation<IHostApi, EasyRenderApi>();
        }
    }
}