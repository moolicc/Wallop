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

            // Note that the results from RunAction must be enumerated over PRIOR to the 
            //     Providers = engineEndPointPluginContext.GetScriptEngineProviders();
            // line being executed.
            // RunAction yield returns its results, so unless enumerated, no providers will be loaded.
            var results = pluginContext.RunAction<ILoadingScriptEnginesEndPoint>(engineEndPointPluginContext);
            foreach (var result in results)
            {
                if(!result.Success)
                {
                    EngineLog.For<ScriptEngineProviderCache>().Error("'{plugin}' plugin failed. {exception}", result.PluginId, result.Exception);
                }
            }

            Providers = engineEndPointPluginContext.GetScriptEngineProviders();
            EngineLog.For<ScriptEngineProviderCache>().Info("{engines} ScriptEngines found.", Providers.Count());
        }
    }
}
