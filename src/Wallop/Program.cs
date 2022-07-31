
using System.CommandLine;
using System.Reflection;
using Wallop;
using Wallop.ECS;

namespace Wallop
{
    class Program
    {
        private const string MUTEX_NAME = "WallopSingleInstanceMutex";
        private const string CONF_FILE = "engineconf.json";

        private static EngineApp? _app;

        static int Main(string[] args)
        {
            using (var mutex = new Mutex(true, MUTEX_NAME, out var isOnlyInstance))
            {
                try
                {
                    if(isOnlyInstance)
                    {
                        var pipe = new IPC.PipeHost("wallop.exe", "FIRSTINSTANCE");
                        pipe.Begin();

                        var host = new IPC.IpcNode(pipe);

                        host.OnDataReceived2 = OtherInstanceMessageReceived;
                    }
                    else if(args.Length > 0)
                    {
                        EngineLog.For<Program>().Warn("Another instance of the application has been detected. Forwarding command line and exiting.");

                        var pipe = new IPC.PipeClient($"wallop.exe-{DateTime.Now.Ticks}", "FIRSTINSTANCE");
                        var client = new IPC.IpcNode(pipe);

                        client.Send(args.Aggregate((s1, s2) => s1 + ' ' + s2), "wallop.exe");
                        if(client.GetReply(TimeSpan.MaxValue, out var reply))
                        {
                            Console.WriteLine(reply);
                        }
                        client.Shutdown();

                        return 0;
                    }
                    else
                    {
                        return 0;
                    }
                    RunProgram();
                }
                catch (Exception ex)
                {
                    EngineLog.For<Program>().Fatal(ex, "Engine has encountered a fatal exception and cannot continue execution. Ex: {exception}", ex);
                    return 1;
                }
            }

            return 0;
        }

        private static void OtherInstanceMessageReceived(IPC.IpcNode node, IPC.IpcMessage message)
        {
            if(_app == null)
            {
                // TODO: Log missed message.
                return;
            }

            EngineLog.For<Program>().Info("Incoming message from application: {sourceApp}.", message.SourceApplication);
            EngineLog.For<Program>().Info("Incoming message content:\n{content}.", message.Content);

            var console = new System.CommandLine.IO.TestConsole();
            _app.ProcessCommandLine(false, message.Content.Trim(), console);

            var results = console.Out.ToString();
            node.Send(results ?? "", message.SourceApplication);
        }

        private static void RunProgram()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LoadDep);

            EngineLog.For<Program>().Info("Loading configuration...");
            var engineConfig = LoadSettings();
            EngineLog.For<Program>().Info("Configuration loaded");

            EngineLog.For<Program>().Info("Resolving config bindings...");
            engineConfig.ResolveBindingsAsync<Settings.GraphicsSettings>().Wait();
            engineConfig.ResolveBindingsAsync<Settings.SceneSettings>().Wait();
            foreach (var item in engineConfig.GetValues())
            {
                EngineLog.For<Program>().Debug("{key}: {value}", item.Key, item.Value);
            }
            EngineLog.For<Program>().Info("Resolved.");

            EngineLog.For<Program>().Info("Loading plugin assemblies...");
            var pluginLoader = new PluginPantry.PluginLoader();
            //var plugins = pluginLoader.LoadPluginAssembly(@"C:\Users\joel\source\repos\moolicc\Wallop\Plugins\TestPlugin\bin\Debug\net6.0\TestPlugin.dll");
            var morePlugins = pluginLoader.LoadPluginAssembly(@"C:\Users\joel\source\repos\Wallop\src\Plugins\EnginePlugins\bin\Debug\net6.0\EnginePlugins.dll");
            var yetMore = pluginLoader.LoadPluginAssembly(@"C:\Users\joel\source\repos\Wallop\src\Plugins\HostApis\bin\Debug\net6.0\HostApis.dll");
            var moarPlugions = pluginLoader.LoadPluginAssembly(@"C:\Users\joel\source\repos\Wallop\src\Plugins\Scripting.IronPython\bin\Debug\net6.0\Scripting.IronPython.dll");
            var anotherone = pluginLoader.LoadPluginAssembly(@"C:\Users\joel\source\repos\Wallop\src\Plugins\StandardComponents\bin\Debug\net6.0\StandardComponents.dll");


            EngineLog.For<Program>().Info("Including plugins into context...");
            var context = new PluginPantry.PluginContext();
            //context.IncludePlugins(plugins);
            context.IncludePlugins(morePlugins);
            context.IncludePlugins(yetMore);
            context.IncludePlugins(moarPlugions);
            context.IncludePlugins(anotherone);


            EngineLog.For<Program>().Info("Running plugin entry points...");
            context.BeginPluginExecution(new Types.Plugins.EndPoints.EntryPointContext());


            using(_app = new EngineApp(engineConfig, context))
            {
                EngineLog.For<Program>().Info("Setting up Engine...");


                _app.ProcessCommandLine(true, Environment.CommandLine);

                EngineLog.For<Program>().Info("Running Engine...");
                _app.Run();
            }
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
                @"C:\Users\joel\source\repos\Wallop\src\Plugins\Scripting.IronPython\bin\Debug\net6.0",
                @"C:\Users\joel\source\repos\Wallop\src\Plugins\HostApis\bin\Debug\net6.0",
                @"C:\Users\joel\source\repos\Wallop\src\Plugins\EnginePlugins\bin\Debug\net6.0",
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