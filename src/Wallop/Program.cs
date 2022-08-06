
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Reflection;
using Wallop;
using Wallop.ECS;

namespace Wallop
{
    class Program
    {
        public const string APP_RESOURCE_DELIMITER = "-";
        public const string DEFAULT_NAME = "wallop.exe";
        public const string MY_NAME_DIRECTIVE = "my-name";
        public const string SEND_TO_DIRECTIVE = "send-to";

        private const string DEFAULT_CONF = "engine.json";

        private const string ARGS_PIPE_RESOURCE = "args";
        private const string MUTEX_RESOURCE = "single-instance-mutex";

        private static EngineApp? _app;
        private static string _confFile = DEFAULT_CONF;
        private static Cog.Configuration _config;
        private static Settings.AppSettings _appSettings;
        private static Settings.PluginSettings _pluginSettings;

        static int Main(string[] args)
        {
            string targetApp = DEFAULT_NAME;
            if(args.Length >= 1)
            {
                if (args[0].StartsWith($"[{SEND_TO_DIRECTIVE}"))
                {
                    if (args.Length == 1)
                    {
                        return 0;
                    }
                    targetApp = args[0].TrimEnd(']').Substring(args[0].IndexOf(':') + 1);
                    args = args[1..];
                }
                else if (args[0].StartsWith($"[{MY_NAME_DIRECTIVE}"))
                {
                    targetApp = args[0].TrimEnd(']').Substring(args[0].IndexOf(':') + 1);
                    args = args[1..];

                    Settings.AppSettings.InstanceName = targetApp;
                }
            }

            using (var mutex = new Mutex(true, $"{targetApp}{APP_RESOURCE_DELIMITER}{MUTEX_RESOURCE}", out var isOnlyInstance))
            {
                try
                {
                    if(isOnlyInstance)
                    {
                        var pipe = new IPC.PipeHost($"{Settings.AppSettings.InstanceName}", $"{Settings.AppSettings.InstanceName}{APP_RESOURCE_DELIMITER}{ARGS_PIPE_RESOURCE}");
                        pipe.Begin();

                        var host = new IPC.IpcNode(pipe);

                        host.OnDataReceived2 = OtherInstanceMessageReceived;
                    }
                    else if(!isOnlyInstance && args.Length > 0)
                    {
                        EngineLog.For<Program>().Warn("Another instance of the application has been detected. Forwarding command line and exiting.");

                        var targetResource = ARGS_PIPE_RESOURCE;


                        var pipe = new IPC.PipeClient($"{DEFAULT_NAME}-{DateTime.Now.Ticks}", $"{targetApp}{APP_RESOURCE_DELIMITER}{targetResource}");
                        var client = new IPC.IpcNode(pipe);

                        client.Send(args.Aggregate((s1, s2) => s1 + ' ' + s2), targetApp);
                        if(client.GetReply(TimeSpan.MaxValue, out var reply))
                        {
                            Console.WriteLine(reply);
                        }
                        client.Shutdown();

                        return 0;
                    }
                    else if(!isOnlyInstance && args.Length <= 0)
                    {
                        return 0;
                    }
                    RunProgram(isOnlyInstance);
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
            ExecuteCommandLine(false, message.Content.Trim(), console);

            var results = console.Out.ToString();
            node.Send(results ?? "", message.SourceApplication);
        }

        private static void RunProgram(bool isFirstInstance)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LoadDep);

            var commandLine = "";
            if(Environment.GetCommandLineArgs().Length > 1)
            {
                commandLine = string.Join(" ", Environment.GetCommandLineArgs()[1..]);
            }
            ExecuteCommandLine(isFirstInstance, commandLine, null);


            var context = new PluginPantry.PluginContext();
            LoadPlugins(context);


            EngineLog.For<Program>().Info("Running plugin entry points...");
            context.BeginPluginExecution(new Types.Plugins.EndPoints.EntryPointContext());



            using (_app = new EngineApp(_config, context))
            {
                EngineLog.For<Program>().Info("Setting up Engine Application...");

                isFirstInstance = !isFirstInstance;
                ExecuteCommandLine(isFirstInstance, commandLine, null);

                EngineLog.For<Program>().Info("Running Engine...");
                _app.Run();
            }
        }

        private static void LoadSettings()
        {
            EngineLog.For<Program>().Info("Loading configuration...");
            var typedSource = new Cog.Sources.TypedSettingSource();
            var jsonSource = new Cog.Sources.JsonSettingsSource(_confFile);
            var engineConfig = new Cog.Configuration();

            engineConfig.Options.Sources.Add(typedSource);
            engineConfig.Options.Sources.Add(jsonSource);
            engineConfig.Options.ConfigureBindings = false;

            engineConfig.LoadSettingsAsync().Wait();

            _config = engineConfig;
        }

        private static void ResolveSettingBindings()
        {
            EngineLog.For<Program>().Info("Resolving config bindings...");
            _config.ResolveBindingsAsync<Settings.GraphicsSettings>().Wait();
            _config.ResolveBindingsAsync<Settings.SceneSettings>().Wait();
            _config.ResolveBindingsAsync<Settings.AppSettings>().Wait();
            _config.ResolveBindingsAsync<Settings.PluginSettings>().Wait();
            foreach (var item in _config.GetValues())
            {
                EngineLog.For<Program>().Debug("{key}: {value}", item.Key, item.Value);
            }

            _appSettings = _config.Get<Settings.AppSettings>() ?? new Settings.AppSettings();
            _pluginSettings = _config.Get<Settings.PluginSettings>() ?? new Settings.PluginSettings();


            var settings = _config.Get<Settings.SceneSettings>() ?? new Settings.SceneSettings();
            if (!Directory.Exists(settings.PackageSearchDirectory))
            {
                Directory.CreateDirectory(settings.PackageSearchDirectory);
            }
        }

        private static void LoadPlugins(PluginPantry.PluginContext context)
        {
            EngineLog.For<Program>().Info("Loading plugins...");
            var pluginLoader = new PluginPantry.PluginLoader();

            int pluginCount = 0;
            int enabledPlugins = 0;

            foreach (var plugin in _pluginSettings.Plugins)
            {
                pluginCount++;
                if(plugin.PluginEnabled)
                {

                    if(!File.Exists(plugin.PluginDll))
                    {
                        EngineLog.For<Program>().Error("Plugin assembly not found. Path: {path}.", plugin.PluginDll);
                        continue;
                    }

                    enabledPlugins++;
                    var newPlugins = pluginLoader.LoadPluginAssembly(plugin.PluginDll);
                    context.IncludePlugins(newPlugins);
                }
            }

            EngineLog.For<Program>().Info("{enabled} plugins enabled out of {loaded} total plugins loaded.", enabledPlugins, pluginCount);
        }

        private static void ExecuteCommandLine(bool firstInstance, string commandLine, IConsole? console)
        {
            RootCommand root;
            if(_app != null)
            {
                root = _app.BuildCommandTree(firstInstance);
            }
            else
            {
                root = new RootCommand();
            }
            BuildProgramCommandTree(root);

            var builder = new CommandLineBuilder(root);
            builder.AddMiddleware(async (context, next) =>
            {
                if (context.ParseResult.Directives.TryGetValues(MY_NAME_DIRECTIVE, out var values))
                {
                    if (values.Count != 1)
                    {
                        //TODO: Error
                    }
                    //Settings.AppSettings.InstanceName = values[0];
                }
                await next(context);
            });
            builder.UseDefaults();
            builder.Build().Invoke(commandLine.Trim(), console);

            //root.Invoke(commandLine.Trim(), console);

            if(firstInstance)
            {
                //var pipe = new IPC.PipeHost($"{Settings.AppSettings.InstanceName}", $"{Settings.AppSettings.InstanceName}{APP_RESOURCE_DELIMITER}{ARGS_PIPE_RESOURCE}");
                //pipe.Begin();

                //var host = new IPC.IpcNode(pipe);

                //host.OnDataReceived2 = OtherInstanceMessageReceived;
            }
        }

        private static void BuildProgramCommandTree(RootCommand root)
        {
            var confOption = new Option<string>(new[] { "--configuration", "-c" }, new Func<string>(() => _confFile), description: "The engine configuration to load.");
            root.Add(confOption);

            root.SetHandler((c) =>
            {
                if(c != null)
                {
                    _confFile = c;
                }
                LoadSettings();
                ResolveSettingBindings();
            }, confOption);
        }

        private static Assembly? LoadDep(object sender, ResolveEventArgs e)
        {
            EngineLog.For<Program>().Info("Unloaded dependency found: <{dependency}> on behalf of <{requester}>.", e.Name, e.RequestingAssembly?.GetName().Name ?? "[Not specified]");

            var searchDirectories = GetDependencyDirectories();

            EngineLog.For<Program>().Debug("Searching in {topDirectories} dependency directories (top-level only).", _appSettings.DependencyPaths.Length);


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

        private static IEnumerable<string> GetDependencyDirectories()
        {
            foreach (var item in _appSettings.DependencyPaths)
            {
                if (item.Recursive)
                {
                    var directories = Directory.GetDirectories(item.Directory, "*", SearchOption.AllDirectories);
                    foreach (var subDir in directories)
                    {
                        yield return subDir;
                    }
                }
                yield return item.Directory;
            }
        }
    }
}