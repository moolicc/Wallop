using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using Wallop.DSLExtension.Scripting;
using Wallop.DSLExtension.Types.Plugin;
using Wallop.Engine.Handlers;
using Wallop.Engine.Messaging;
using Wallop.Engine.Messaging.Messages;
using Wallop.Engine.SceneManagement;
using Wallop.Engine.SceneManagement.Serialization;
using Wallop.Engine.Scripting;
using Wallop.Engine.Settings;
using Wallop.Engine.Types.Plugins;
using Wallop.Engine.Types.Plugins.EndPoints;


// TODO:
// Write a thread-safe rendering mechanism that can be shared by all resources contained in a Layout.
// Write a feature-rich set of HostApis in the plugin of the same name

// Create an additional handler for extension points.
//   A SceneHandler shouldn't control a taskprovider for example (it currently does).
// Unify what is a service versus just a Handler's class variable.
// Cleanup and unify initialization. Make handlers less hard coded and more abstract from the consumer's point of view.


namespace Wallop.Engine
{
    // TODO: Ensure we're using SDL and not GLFW
    public class EngineApp : IDisposable
    {
        public Messenger Messenger => _messenger;


        private List<object> _services;

        private PluginPantry.PluginContext _pluginContext;

        private bool _sceneSetup;

        private Messenger _messenger;
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

        public void ProcessCommandLine(bool firstInstance, string commands)
        {
            var engineConf = new Option<string>(
                new[] { "--conf", "-c" },
                () => "engineconf.json",
                "Specifies the engine configuratiun file");

            RootCommand root = new RootCommand
            {
                engineConf,
            };

            foreach (var handler in _handlers)
            {
                if (handler.GetCommandLineCommand(firstInstance) is Command cmd)
                {
                    root.AddCommand(cmd);
                }
            }

            root.Invoke(commands.Trim());
        }

        public void WindowLoaded()
        {
            SetupScene();
        }

        private void SetupScene()
        {
            EngineLog.For<EngineApp>().Info("Warming things up...");
            AddService(new ScriptEngineProviderCache(_pluginContext));
            LoadBindableTypes();

            _sceneHandler.InitScene();

            _sceneSetup = true;
            EngineLog.For<EngineApp>().Info("Setup complete!");
        }

        private void LoadBindableTypes()
        {
            EngineLog.For<EngineApp>().Info("Initializing BindableType registrations...");
            var bindableConext = new BindableTypeEndPoint();
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