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
            pluginInfo.Exposed.RegisterEndPoint<IInjectScriptContextEndPoint>(nameof(InjectScriptContext), this, pluginInfo.PluginId);
        }

        public void InjectScriptContext(ScriptContext context)
        {
            context.AddValue("name", "Albert");
        }
    }
}