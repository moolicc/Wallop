using PluginPantry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Scripting;
using Wallop.Shared.Types.Plugin;

namespace Scripting.IronPython
{
    public class ScriptPlugin
    {

        [EntryPoint(new string[] { "name", "Scripting", "version", "1.0.0.0" })]
        public static void Startup(PluginContext context, Guid id)
        {
            context.RegisterAction<ILoadingScriptEnginesEndPoint, ScriptPlugin>(id, nameof(LoadEngines));
            context.RegisterAction<IInjectScriptContextEndPoint, ScriptPlugin>(id, nameof(InjectScriptContext));
        }

        public static void LoadEngines(ILoadingScriptEnginesEndPoint endPoint)
        {
            endPoint.RegisterScriptEngineProvider(new PythonScriptEngineProvider());
        }

        public static void InjectScriptContext(IScriptContext context)
        {
        }
    }
}
