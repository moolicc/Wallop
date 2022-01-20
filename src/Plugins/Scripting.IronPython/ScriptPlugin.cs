using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Scripting;
using Wallop.DSLExtension.Types.Plugin;

namespace Scripting.IronPython
{
    public class ScriptPlugin
    {

        [PluginPantry.Extending.PluginEntryPoint("Scripting", "1.0.0.0")]
        public void Startup(PluginPantry.Extending.PluginInformation pluginInfo)
        {
            pluginInfo.Exposed.RegisterEndPoint<ILoadingScriptEnginesEndPoint>(nameof(LoadEngines), this, pluginInfo.PluginId);
            pluginInfo.Exposed.RegisterEndPoint<IInjectScriptContextEndPoint>(nameof(InjectScriptContext), this, pluginInfo.PluginId);
        }

        public void LoadEngines(ILoadingScriptEnginesEndPoint endPoint)
        {
            endPoint.RegisterScriptEngineProvider(new PythonScriptEngineProvider());
        }

        public void InjectScriptContext(ScriptContext context)
        {
            context.AddValue("name", "Albert");
        }
    }
}
