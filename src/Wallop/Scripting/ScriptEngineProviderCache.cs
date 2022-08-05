using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Scripting;
using Wallop.Shared.Types.Plugin;
using Wallop.Types.Plugins.EndPoints;

namespace Wallop.Scripting
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
