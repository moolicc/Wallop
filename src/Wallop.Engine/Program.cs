
using System.Reflection;
using Wallop.Engine;

namespace Wallop
{

    class Program
    {
        private const string CONF_FILE = "engineconf.json";

        static int Main()
        {
            EngineLog.For<Program>().Info("Loading configuration...");
            var engineConfig = LoadSettings();
            EngineLog.For<Program>().Info("Configuration loaded");

            EngineLog.For<Program>().Info("Resolving config bindings...");
            engineConfig.ResolveBindingsAsync<Engine.Settings.GraphicsSettings>().Wait();
            engineConfig.ResolveBindingsAsync<Engine.Settings.SceneSettings>().Wait();
            foreach (var item in engineConfig.GetValues())
            {
                EngineLog.For<Program>().Debug("{key}: {value}", item.Key, item.Value);
            }
            EngineLog.For<Program>().Info("Resolved.");


            EngineLog.For<Program>().Info("Loading plugin assemblies...");
            var pluginLoader = new PluginPantry.PluginLoader();
            //var plugins = pluginLoader.LoadPluginAssembly(@"C:\Users\joel\source\repos\moolicc\Wallop\Plugins\TestPlugin\bin\Debug\net6.0\TestPlugin.dll");
            var morePlugins = pluginLoader.LoadPluginAssembly(@"C:\Users\joel\source\repos\moolicc\Wallop\src\Plugins\EnginePlugins\bin\Debug\net6.0\EnginePlugins.dll");
            var yetMore = pluginLoader.LoadPluginAssembly(@"C:\Users\joel\source\repos\moolicc\Wallop\src\Plugins\HostApis\bin\Debug\net6.0\HostApis.dll");
            var moarPlugions = pluginLoader.LoadPluginAssembly(@"C:\Users\joel\source\repos\moolicc\Wallop\src\Plugins\Scripting.IronPython\bin\Debug\net6.0\Scripting.IronPython.dll");
            var anotherone = pluginLoader.LoadPluginAssembly(@"C:\Users\joel\source\repos\moolicc\Wallop\src\Plugins\StandardComponents\bin\Debug\net6.0\StandardComponents.dll");


            EngineLog.For<Program>().Info("Including plugins into context...");
            var context = new PluginPantry.PluginContext();
            //context.IncludePlugins(plugins);
            context.IncludePlugins(morePlugins);
            context.IncludePlugins(yetMore);
            context.IncludePlugins(moarPlugions);
            context.IncludePlugins(anotherone);


            EngineLog.For<Program>().Info("Running plugin entry points...");
            context.BeginPluginExecution(new Wallop.Engine.Types.Plugins.EndPoints.EntryPointContext());

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LoadDep);

            EngineLog.For<Program>().Info("Setting up Engine...");
            var app = new EngineApp(engineConfig, context);
            app.SetupWindow();

            EngineLog.For<Program>().Info("Running Engine...");
            app.Run();

            return 0;
        }

        private static Cog.Configuration LoadSettings()
        {
            var typedSource = new Cog.Sources.TypedSettingSource();
            var jsonSource = new Cog.Sources.JsonSettingsSource(CONF_FILE);
            var engineConfig = new Cog.Configuration();

            engineConfig.Options.Sources.Add(typedSource);
            engineConfig.Options.Sources.Add(jsonSource);
            engineConfig.Options.ConfigureBindings = false;

            engineConfig.LoadSettingsAsync().Wait();

            return engineConfig;
        }

        private static Assembly? LoadDep(object sender, ResolveEventArgs e)
        {
            EngineLog.For<Program>().Info("Unloaded dependency found: <{dependency}> on behalf of <{requester}>.", e.Name, e.RequestingAssembly?.GetName().Name ?? "[Not specified]");

            var searchDirectories = new[]
            {
                @"C:\Users\joel\source\repos\moolicc\Wallop\src\Plugins\Scripting.IronPython\bin\Debug\net6.0",
                @"C:\Users\joel\source\repos\moolicc\Wallop\src\Plugins\HostApis\bin\Debug\net6.0",
            };

            EngineLog.For<Program>().Debug("Searching in {numDir} directories.", searchDirectories.Length);


            int subLength = e.Name.Length;
            if (e.Name.Contains(','))
            {
                subLength = e.Name.IndexOf(',');
            }
            var fileName = $"{e.Name.Substring(0, subLength)}.dll";

            EngineLog.For<Program>().Debug("Searching for reference assembly {file} to resolve...", fileName);

            foreach (var dir in searchDirectories)
            {
                EngineLog.For<Program>().Debug("Searching for reference assembly in search directory {dir}...", dir);
                var combined = Path.Combine(dir, fileName);
                if (File.Exists(combined))
                {
                    EngineLog.For<Program>().Info("Unloaded dependency found at {assemblyPath}!", combined);
                    return Assembly.LoadFile(combined);
                }
            }

            EngineLog.For<Program>().Warn("Unloaded dependency could not be located!");
            return null;
        }
    }
}