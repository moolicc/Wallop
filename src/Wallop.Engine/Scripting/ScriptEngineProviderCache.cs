using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Scripting;
using Wallop.DSLExtension.Types.Plugin;
using Wallop.Engine.Types.Plugins.EndPoints;

namespace Wallop.Engine.Scripting
{
    public class ScriptEngineProviderCache
    {
        public IEnumerable<IScriptEngineProvider> Providers { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ScriptEngineProviderCache(EngineApp app, PluginPantry.PluginContext pluginContext)
        {
            RefreshProviders(app, pluginContext);
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public void RefreshProviders(EngineApp app, PluginPantry.PluginContext pluginContext)
        {
            EngineLog.For<ScriptEngineProviderCache>().Info("Initializing ScriptEngines providers...");

            var engineEndPointPluginContext = new ScriptEngineEndPoint(app.Messenger);
            pluginContext.ExecuteEndPoint<ILoadingScriptEnginesEndPoint>(engineEndPointPluginContext);
            pluginContext.WaitForEndPointExecutionAsync<ILoadingScriptEnginesEndPoint>().WaitAndThrow();
            Providers = engineEndPointPluginContext.GetScriptEngineProviders();
            EngineLog.For<ScriptEngineProviderCache>().Info("{engines} ScriptEngines found.", Providers.Count());
        }
    }
}
