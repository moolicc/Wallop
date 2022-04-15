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


namespace Wallop.Engine
{
    // TODO: Ensure we're using SDL and not GLFW
    public class EngineApp : IDisposable
    {
        public GL GL => _gl;
        public IWindow Window => _window;
        public GraphicsSettings GraphicsSettings => _graphicsSettings;
        public Messenger Messenger => _messenger;
        public PackageCache PackageCache => _packageCache;
        public TaskHandlerProvider TaskHandlerProvider => _taskProvider;


        //private GraphicsDevice _graphicsDevice;
        private GL _gl;
        private IWindow _window;

        private ScriptHostFunctions _scriptHostFunctions;
        private ScriptEngineProviderCache _scriptEngineProviders;

        private GraphicsSettings _graphicsSettings;


        private PluginPantry.PluginContext _pluginContext;

        // TODO: Turn this into another XCache type.
        private IEnumerable<KeyValuePair<string, Type>> _bindableComponentTypes;
        private TaskHandlerProvider _taskProvider;

        private List<object> _services;

        private bool _sceneSetup;

        private Messenger _messenger;
        private SceneHandler _sceneHandler;
        private List<EngineHandler> _handlers;

        private PackageCache _packageCache;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public EngineApp(Cog.Configuration config, PluginPantry.PluginContext pluginContext)
        {
            EngineLog.For<EngineApp>().Debug("Loading configurations...");
            _graphicsSettings = config.Get<GraphicsSettings>() ?? new GraphicsSettings();

            _pluginContext = pluginContext;

            _sceneSetup = false;


            var graphicsHandler = new GraphicsHandler(this);
            _sceneHandler = new SceneHandler(this, config.Get<SceneSettings>() ?? new SceneSettings());

            _messenger = new Messenger();

            _handlers = new List<EngineHandler>();
            _handlers.Add(graphicsHandler);
            _handlers.Add(_sceneHandler);


            _packageCache = new PackageCache(_sceneHandler.SceneSettings.PackageSearchDirectory);


            _services = new List<object>();
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public T? GetHandler<T>() where T : EngineHandler
            => (T?)_handlers.FirstOrDefault(h => h.GetType() == typeof(T));

        public T? GetService<T>()
            => (T?)_services.FirstOrDefault(h => h.GetType() == typeof(T));

        public void SetupWindow()
        {
            EngineLog.For<EngineApp>().Debug("Executing plugins on EngineStartup...");
            _pluginContext.ExecuteEndPoint(new EngineStartupEndPoint { GraphicsSettings = _graphicsSettings });


            EngineLog.For<EngineApp>().Info("Creating window with options: {options}", _graphicsSettings);
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(_graphicsSettings.WindowWidth, _graphicsSettings.WindowHeight);
            options.WindowBorder = _graphicsSettings.WindowBorder;
            options.IsVisible = _graphicsSettings.SkipOverlay;
            options.FramesPerSecond = _graphicsSettings.RefreshRate;
            options.UpdatesPerSecond = _graphicsSettings.RefreshRate;
            options.VSync = _graphicsSettings.VSync;
            options.ShouldSwapAutomatically = true;

            Silk.NET.Windowing.Window.PrioritizeSdl();
            _window = Silk.NET.Windowing.Window.Create(options);
            _window.Load += WindowLoad;
            _window.FramebufferResize += WindowResized;
            _window.Update += Update;
            _window.Render += Draw;
            _window.Closing += Shutdown;
        }

        public void Run()
        {
            EngineLog.For<EngineApp>().Info("Beginning Engine Execution!");
            _window.Run();
        }

        public void ProcessCommandLine(string commands)
        {
            var engineConf = new Option<string>(
                new[] { "--conf", "-c" },
                () => "engineconf.json",
                "Specifies the engine configuratiun file");

            var windowWidth = new Option<int>(
                new[] { "--win-width", "-ww" },
                () => _graphicsSettings.WindowWidth,
                "Specifies the width of the underlying window.");
            var windowHeight = new Option<int>(
                new[] { "--win-height", "-wh" },
                () => _graphicsSettings.WindowHeight,
                "Specifies the height of the underlying window.");
            var windowBorder = new Option<WindowBorder>(
                new[] { "--win-border", "-wb" },
                () => _graphicsSettings.WindowBorder,
                "Specifies the border style of the underlying window.");
            var skipOverlayOpt = new Option<bool>(
                new[] { "--overlay", "-o" },
                () => _graphicsSettings.SkipOverlay,
                "Specifies whether or not to overlay the window over the desktop.");
            var refreshRateOpt = new Option<double>(
                new[] { "--refresh-rate", "-r" },
                () => _graphicsSettings.RefreshRate,
                "Specifies the refresh rate.");
            var vsyncEnable = new Option<bool>(
                new[] { "--vsync", "-v" },
                () => _graphicsSettings.VSync,
                "Specifies if vsync should be enabled or disabled.");



            // EngineApp.exe graphics ...
            var graphicsCommand = new Command("graphics", "Graphics operations")
            {
                windowWidth,
                windowHeight,
                windowBorder,
                skipOverlayOpt,
                refreshRateOpt,
                vsyncEnable,
            };

            RootCommand root = new RootCommand
            {
                engineConf,
                graphicsCommand,
            };

            foreach (var handler in _handlers)
            {
                var cmd = handler.GetCommandLineCommand();
                if (cmd != null)
                {
                    root.AddCommand(cmd);
                }
            }

            var graphicsHandler = CommandHandler.Create<int, int, WindowBorder, bool, double, bool>(
                (winWidth, winHeight, winBorder, overlay, refreshRate, vsync) =>
                {
                    var changes = new Settings.GraphicsSettings();
                    changes.WindowWidth = winWidth;
                    changes.WindowHeight = winHeight;
                    changes.WindowBorder = winBorder;
                    changes.SkipOverlay = overlay;
                    changes.RefreshRate = refreshRate;
                    changes.VSync = vsync;

                    if (_sceneSetup)
                    {
                        _messenger.Put(new GraphicsMessage() { ChangeSet = changes });
                    }
                    else
                    {
                        _graphicsSettings = changes;
                    }
                });



            var sceneCreateHandler = CommandHandler.Create<string, string>(
                (clone, name) =>
                {
                    // TODO: Put the create message onto the messenger.
                    // _messenger.Put();
                });

            var sceneCreateLayoutHandler = CommandHandler.Create<string, string, string, bool>(
                (clone, name, targetScene, makeActive) =>
                {
                    _messenger.Put(new AddLayoutMessage(name, clone, targetScene, makeActive));
                });

            graphicsCommand.Handler = graphicsHandler;

            root.Invoke(commands.Trim());
        }

        private void WindowLoad()
        {
            //_graphicsDevice = new GraphicsDevice(GL.GetApi(_window));
            //var gl = _graphicsDevice.Information;
            _gl = GL.GetApi(_window);

            if (!_graphicsSettings.SkipOverlay)
            {
                EngineLog.For<EngineApp>().Info("Running execution of Engine Overlay plugin...");
                _pluginContext.ExecuteEndPoint(new OverlayerEndPoint(_window));
                _pluginContext.WaitForEndPointExecutionAsync<OverlayerEndPoint>().ContinueWith(_ =>
                {
                    _window.IsVisible = true;
                });
            }
            else
            {
                EngineLog.For<EngineApp>().Info("Skipping execution of Engine Overlay plugin due to settings specified in configuration.");
            }

            SetupMessageQueues();
            SetupScene();
            _gl.Viewport(_window.Size);
        }

        private void WindowResized(Vector2D<int> size)
        {
            //_graphicsDevice.GetOpenGLInstance().Viewport(size);
            _gl.Viewport(size);
        }

        private void SetupMessageQueues()
        {
            _messenger.RegisterQueue<GraphicsMessage>();
            _messenger.RegisterQueue<SceneChangeMessage>();
        }

        private void SetupScene()
        {
            EngineLog.For<EngineApp>().Info("Warming things up...");
            LoadScriptEngineProviders();

            _scriptHostFunctions = new ScriptHostFunctions();
            _sceneHandler.InitScene();

            _sceneSetup = true;
            EngineLog.For<EngineApp>().Info("Setup complete!");
        }

        private void LoadScriptEngineProviders()
        {
            _scriptEngineProviders = new ScriptEngineProviderCache(_pluginContext);

            EngineLog.For<EngineApp>().Info("Initializing BindableType registrations...");
            var bindableConext = new BindableTypeEndPoint();
            _pluginContext.ExecuteEndPoint<IBindableTypeRegistrationEndPoint>(bindableConext);
            _pluginContext.WaitForEndPointExecutionAsync<IBindableTypeRegistrationEndPoint>().WaitAndThrow();
            _bindableComponentTypes = bindableConext.BindableTypes;
            EngineLog.For<EngineApp>().Info("{types} BindableTypes found.", _bindableComponentTypes.Count());
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

            _gl.Clear(ClearBufferMask.ColorBufferBit);
            _sceneHandler.SceneDraw();


            foreach (var handler in _handlers)
            {
                handler.AfterDraw();
            }
        }

        public void UpdateGraphics()
        {
            _gl.Viewport(_window.Size);
        }

        public void Dispose()
        {
        }

        public void Shutdown()
        {
            EngineLog.For<EngineApp>().Info("Shutting down Engine...");
            EngineLog.For<EngineApp>().Info("...Scene...");
            _sceneHandler.SceneShutdown();
            if (!_window.IsClosing)
            {
                _window.Closing -= Shutdown;
                _window.Close();
                return;
            }
        }
    }
}