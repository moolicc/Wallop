using Silk.NET.Windowing;
using System.CommandLine;
using System.CommandLine.Parsing;
using Wallop.Shared.ECS;
using Wallop.Shared.ECS.Serialization;
using Wallop.Shared.Modules;
using Wallop.Shared.Scripting;
using Wallop.Handlers;
using Wallop.Shared.Messaging;
using Wallop.Shared.Messaging.Messages;
using Wallop.Shared.Messaging.Json;
using Wallop.Scripting;
using Wallop.Settings;
using Wallop.Shared.Types.Plugin;
using Wallop.Types.Plugins.EndPoints;
using Wallop.Shared.Messaging.Remoting;


// TODO:
// Write a thread-safe rendering mechanism that can be shared by all resources contained in a Layout.
// Write a feature-rich set of HostApis in the plugin of the same name

// Create an additional handler for extension points.
//   A SceneHandler shouldn't control a taskprovider for example (it currently does).
// Unify what is a service versus just a Handler's class variable.
// Cleanup and unify initialization. Make handlers less hard coded and more abstract from the consumer's point of view.


namespace Wallop
{
    // TODO: Ensure we're using SDL and not GLFW
    public class EngineApp : IDisposable
    {
        public Messenger Messenger => _messenger;


        private List<object> _services;

        private PluginPantry.PluginContext _pluginContext;

        private bool _sceneSetup;

        private Messenger _messenger;
        private IPC.PipeHost _relayHost;
        private IPC.IpcNode _relayNode;
        private MessageRelay _relay;
        private GraphicsHandler _graphicsHandler;
        private SceneHandler _sceneHandler;
        private List<EngineHandler> _handlers;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public EngineApp(Cog.Configuration config, PluginPantry.PluginContext pluginContext)
        {
            _handlers = new List<EngineHandler>();
            _services = new List<object>();
            _messenger = new Messenger();

            _pluginContext = pluginContext;

            AddService(_pluginContext);
            AddService(new ScriptHostFunctions());

            EngineLog.For<EngineApp>().Debug("Loading handler configurations...");
            _graphicsHandler = new GraphicsHandler(this, config.Get<GraphicsSettings>() ?? new GraphicsSettings());
            _sceneHandler = new SceneHandler(this, config.Get<SceneSettings>() ?? new SceneSettings());


            _handlers.Add(_graphicsHandler);
            _handlers.Add(_sceneHandler);
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public T? GetHandler<T>() where T : EngineHandler
            => (T?)_handlers.FirstOrDefault(h => h.GetType() == typeof(T));

        public T? GetService<T>()
            => (T?)_services.FirstOrDefault(h => h.GetType() == typeof(T));

        public void AddService<T>(T service)
        {
            if(service is not null)
            {
                _services.Add(service);
            }
            else
            {
                throw new ArgumentNullException(nameof(service));
            }
        }



        public void Run()
        {
            EngineLog.For<EngineApp>().Info("Beginning Engine Execution!");


            _graphicsHandler.RunWindow();
        }

        public RootCommand BuildCommandTree(bool firstInstance)
        {
            var startupEndPoint = new EngineStartupEndPoint(Messenger);
            _pluginContext.ExecuteEndPoint(startupEndPoint);
            _pluginContext.WaitForEndPointExecutionAsync<EngineStartupEndPoint>().Wait();


            RootCommand root = new RootCommand();


            var jsonArg = new Argument<string>("source", "Json text or a path to a json file.");
            var jsonCommand = new Command("json", "Pass messages to the app through json.")
            {
                jsonArg
            };
            jsonCommand.SetHandler<string>(j =>
            {
                if(File.Exists(j))
                {
                    j = File.ReadAllText(j);
                }

                var messages = Json.ParseMessages(j);

                foreach (var item in messages)
                {
                    Messenger.Put(item.Value, item.MessageType);
                }

            }, jsonArg);


            foreach (var handler in _handlers)
            {
                if (handler.GetCommandLineCommand(firstInstance) is Command cmd)
                {
                    root.AddCommand(cmd);
                }
            }

            foreach (var item in startupEndPoint.CommandLineVerbs)
            {
                root.Add(item);
            }

            return root;
        }

        public void WindowLoaded()
        {
            SetupScene();
            _graphicsHandler.ShowWindow();
        }

        private void SetupScene()
        {
            EngineLog.For<EngineApp>().Info("Warming things up...");
            AddService(new ScriptEngineProviderCache(this, _pluginContext));
            LoadBindableTypes();

            _sceneHandler.InitScene();

            _sceneSetup = true;
            EngineLog.For<EngineApp>().Info("Setup complete!");
        }

        private void LoadBindableTypes()
        {
            EngineLog.For<EngineApp>().Info("Initializing BindableType registrations...");
            var bindableConext = new BindableTypeEndPoint(Messenger);
            _pluginContext.ExecuteEndPoint<IBindableTypeRegistrationEndPoint>(bindableConext);
            _pluginContext.WaitForEndPointExecutionAsync<IBindableTypeRegistrationEndPoint>().WaitAndThrow();
            var bindableComponentTypes = new BindableComponentTypeCache(bindableConext.BindableTypes);
            AddService(bindableComponentTypes);
            EngineLog.For<EngineApp>().Info("{types} BindableTypes found.", bindableComponentTypes.Count);
        }

        public void Update(double delta)
        {
            if (!_sceneSetup)
            {
                return;
            }
            foreach(var handler in _handlers)
            {
                handler.BeforeUpdate();
            }
            _sceneHandler.SceneUpdate();
            foreach (var handler in _handlers)
            {
                handler.AfterUpdate();
            }
        }

        public void Draw(double delta)
        {
            if (!_sceneSetup)
            {
                return;
            }
            foreach (var handler in _handlers)
            {
                handler.BeforeDraw();
            }

            _graphicsHandler.ClearSurface();
            _sceneHandler.SceneDraw();


            foreach (var handler in _handlers)
            {
                handler.AfterDraw();
            }
        }

        public void Dispose()
        {
        }

        public void Shutdown()
        {
            EngineLog.For<EngineApp>().Info("Shutting down Engine...");

            foreach (var handler in _handlers)
            {
                handler.Shutdown();
            }
        }

    }
}