using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Wallop.DSLExtension.Scripting;
using Wallop.DSLExtension.Types.Plugin;
using Wallop.Engine.Messaging;
using Wallop.Engine.Rendering;
using Wallop.Engine.SceneManagement;
using Wallop.Engine.Scripting;
using Wallop.Engine.Scripting.ECS;
using Wallop.Engine.Types.Plugins;
using Wallop.Engine.Types.Plugins.EndPoints;
using static Wallop.Engine.Scripting.HostData;


// TODO:
// Write a thread-safe rendering mechanism that can be shared by all resources contained in a Layout.
// Write a feature-rich set of HostApis in the plugin of the same name


namespace Wallop.Engine
{
    // TODO: Ensure we're using SDL and not GLFW
    internal class EngineApp : IDisposable
    {
        //private GraphicsDevice _graphicsDevice;
        private GL _gl;
        private IWindow _window;

        private Settings.GraphicsSettings _graphicsSettings;
        private Settings.SceneSettings _sceneSettings;


        private PluginPantry.PluginContext _pluginContext;

        private ScriptHostFunctions _scriptHostFunctions;
        private IEnumerable<IScriptEngineProvider> _scriptEngineProviders;
        private IEnumerable<KeyValuePair<string, Type>> _bindableComponentTypes;


        private SceneStore _sceneStore;
        private Scene _activeScene;
        private bool _sceneSetup;

        private Messaging.Messenger _messenger;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public EngineApp(Cog.Configuration config, PluginPantry.PluginContext pluginContext)
        {
            EngineLog.For<EngineApp>().Debug("Loading configurations...");
            _graphicsSettings = config.Get<Settings.GraphicsSettings>() ?? new Settings.GraphicsSettings();
            _sceneSettings = config.Get<Settings.SceneSettings>() ?? new Settings.SceneSettings();

            _pluginContext = pluginContext;

            _sceneStore = new SceneStore();
            _sceneSetup = false;
            _scriptHostFunctions = new ScriptHostFunctions();

            _messenger = new Messaging.Messenger();
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


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

            Window.PrioritizeSdl();
            _window = Window.Create(options);
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
                "--conf",
                () => "engineconf.json",
                "Specifies the engine configuratiun file");

            var windowWidth = new Option<int>(
                "--win-width",
                () => _graphicsSettings.WindowWidth,
                "Specifies the width of the underlying window.");
            var windowHeight = new Option<int>(
                "--win-height",
                () => _graphicsSettings.WindowHeight,
                "Specifies the height of the underlying window.");
            var windowBorder = new Option<WindowBorder>(
                "--win-border",
                () => _graphicsSettings.WindowBorder,
                "Specifies the border style of the underlying window.");
            var skipOverlayOpt = new Option<bool>(
                "--overlay",
                () => _graphicsSettings.SkipOverlay,
                "Specifies whether or not to overlay the window over the desktop.");
            var refreshRateOpt = new Option<double>(
                "--refresh-rate",
                () => _graphicsSettings.RefreshRate,
                "Specifies the refresh rate.");
            var vsyncEnable = new Option<bool>(
                "--vsync",
                () => _graphicsSettings.VSync,
                "Specifies if vsync should be enabled or disabled.");


            var defaultSceneName = new Option<string>(
                "--default-scene",
                () => _sceneSettings.DefaultSceneName,
                "Specifies the name of the default, empty scene.");
            var drawThreadingPolicy = new Option<ThreadingPolicy>(
                "--thread-policy-draw",
                () => _sceneSettings.DrawThreadingPolicy,
                "Specifies the render threading policy.");
            var pkgSearchDir = new Option<string>(
                "--pkg-dir",
                () => _sceneSettings.PackageSearchDirectory,
                "Specifies the search directory for packages.");
            var scenePreloads = new Option<IEnumerable<string>>(
                "--preload",
                () => _sceneSettings.ScenePreloadList.ToArray(),
                "Specifies the scene(s), by their json configuration filepath(s), to pre-load or reload.");
            var selectedScene = new Option<string>(
                "--active-scene",
                () => _sceneSettings.SelectedScene,
                "Specifies the currently active scene by name.");
            var updateThreadingPolicy = new Option<ThreadingPolicy>(
                "--thread-policy-update",
                () => _sceneSettings.UpdateThreadingPolicy,
                "Specifies the update threading policy.");


            var graphicsCommand = new Command("graphics", "Graphics operations")
            {
                windowWidth,
                windowHeight,
                windowBorder,
                skipOverlayOpt,
                refreshRateOpt,
                vsyncEnable,
            };

            var sceneCommand = new Command("scene", "Scene operations")
            {
                defaultSceneName,
                drawThreadingPolicy,
                pkgSearchDir,
                scenePreloads,
                selectedScene,
                updateThreadingPolicy,
            };

            RootCommand root = new RootCommand
            {
                engineConf,
                graphicsCommand,
                sceneCommand,
            };

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

            var sceneHandler = CommandHandler.Create<string, ThreadingPolicy, string, IEnumerable<string>, string, ThreadingPolicy>(
                (defaultScene, drawThreadingPolicy, pkgDir, preload, activeScene, updateThreadingPolicy) =>
                {
                    var newSettings = _sceneSettings.Clone();
                    newSettings.DefaultSceneName = defaultScene;
                    newSettings.DrawThreadingPolicy = drawThreadingPolicy;
                    newSettings.PackageSearchDirectory = pkgDir;
                    newSettings.SelectedScene = activeScene;
                    newSettings.UpdateThreadingPolicy = updateThreadingPolicy;

                    if ((preload.FirstOrDefault() ?? " ")[0] == '+')
                    {
                        string[] additions = preload.ToArray();
                        additions[0] = additions[0].Substring(1);

                        foreach (var addition in additions)
                        {
                            if(!newSettings.ScenePreloadList.Contains(addition))
                            {
                                newSettings.ScenePreloadList.Add(addition);
                            }
                        }
                    }

                    if (_sceneSetup)
                    {
                        // Send command to queue.
                    }
                });


            graphicsCommand.Handler = graphicsHandler;
            sceneCommand.Handler = sceneHandler;

            //commands = commands.Substring(commands.IndexOf(" ") + 1);
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
            _messenger.RegisterQueue<Messaging.GraphicsMessage>();
        }

        public void SetupScene()
        {
            EngineLog.For<EngineApp>().Info("Warming things up...");
            LoadScriptEngineProviders();
            LoadScene();

            _sceneSetup = true;
            EngineLog.For<EngineApp>().Info("Setup complete!");
        }

        private void LoadScriptEngineProviders()
        {
            EngineLog.For<EngineApp>().Info("Initializing ScriptEngines providers...");

            var engineEndPointPluginContext = new ScriptEngineEndPoint();
            _pluginContext.ExecuteEndPoint<ILoadingScriptEnginesEndPoint>(engineEndPointPluginContext);
            _pluginContext.WaitForEndPointExecutionAsync<ILoadingScriptEnginesEndPoint>().WaitAndThrow();
            _scriptEngineProviders = engineEndPointPluginContext.GetScriptEngineProviders();
            EngineLog.For<EngineApp>().Info("{engines} ScriptEngines found.", _scriptEngineProviders.Count());

            EngineLog.For<EngineApp>().Info("Initializing BindableType registrations...");
            var bindableConext = new BindableTypeEndPoint();
            _pluginContext.ExecuteEndPoint<IBindableTypeRegistrationEndPoint>(bindableConext);
            _pluginContext.WaitForEndPointExecutionAsync<IBindableTypeRegistrationEndPoint>().WaitAndThrow();
            _bindableComponentTypes = bindableConext.BindableTypes;
            EngineLog.For<EngineApp>().Info("{types} BindableTypes found.", _bindableComponentTypes.Count());
        }


        private void LoadScene()
        {
            SetupDefaultScene();

            if(_sceneSettings.SelectedScene != _sceneSettings.DefaultSceneName)
            {
                _sceneStore.Load(_sceneSettings.SelectedScene);
            }

            EngineLog.For<EngineApp>().Info("Preloading {scenes} scene configurations...", _sceneSettings.ScenePreloadList.Count);
            foreach (var item in _sceneSettings.ScenePreloadList)
            {
                if(item == _sceneSettings.SelectedScene || item == _sceneSettings.DefaultSceneName)
                {
                    continue;
                }
                _sceneStore.Load(item);
            }

            SwitchScene(_sceneSettings.SelectedScene);
        }

        private void SetupDefaultScene()
        {
            EngineLog.For<EngineApp>().Info("Setting up default scene...");

            var defaultSettings = new StoredScene()
            {
                Name = _sceneSettings.DefaultSceneName,
                DirectorModules = new List<StoredModule>()
                {
                    new StoredModule()
                    {
                        InstanceName = "DirectorTest",
                        ModuleId = "Director.Test1.0",
                        Settings = new Dictionary<string, string>()
                        {
                            { "height", "100" },
                            { "width", "100" }
                        },
                    },
                },
                Layouts = new List<StoredLayout>()
                {
                    new StoredLayout()
                    {
                        Active = true,
                        Name = "layout1",
                        ActorModules = new List<StoredModule>()
                        {
                            new StoredModule()
                            {
                                InstanceName = "Square",
                                ModuleId = "Square.Test1.0",
                                Settings = new Dictionary<string, string>()
                                {
                                    { "height", "100" },
                                    { "width", "100" }
                                }
                            },
                            new StoredModule()
                            {
                                InstanceName = "Square1",
                                ModuleId = "Square.Test1.0",
                                Settings = new Dictionary<string, string>()
                                {
                                    { "height", "100" },
                                    { "width", "100" },
                                    { "y", "200" }
                                },
                                StoredBindings = new List<StoredBinding>()
                                {
                                    new StoredBinding("PositionComponent", "X", "x"),
                                    new StoredBinding("PositionComponent", "Y", "y"),
                                }
                            }
                        }
                    }
                }
            };
            _sceneStore.Add(defaultSettings);
        }

        private void SwitchScene(string newScene)
        {
            EngineLog.For<EngineApp>().Info("Switching to new scene {scene}.", newScene);
            var settings = _sceneStore.Get(newScene);

            EngineLog.For<EngineApp>().Info("Constructing scene...");
            var sceneLoader = new ScriptedSceneLoader(settings);
            var scene = sceneLoader.LoadFromPackages(_sceneSettings.PackageSearchDirectory);


            EngineLog.For<EngineApp>().Debug("Injecting script HostData...");
            var hostData = new HostData(_gl, scene, _bindableComponentTypes);
            _scriptHostFunctions.AddDependencies(hostData);



            EngineLog.For<EngineApp>().Info("Initializing scene and associated modules...");
            var provider = new TaskHandlerProvider(_sceneSettings.UpdateThreadingPolicy, _sceneSettings.DrawThreadingPolicy, () =>
            {
                //_window.MakeCurrent();
            });

            scene.Init(_pluginContext);
            var initializer = new ScriptedSceneInitializer(_scriptHostFunctions, scene, _pluginContext, provider, _scriptEngineProviders, _bindableComponentTypes);
            initializer.InitializeActorScripts();
            initializer.InitializeDirectorScripts();

            _activeScene = scene;
        }

        public void SaveCurrentSceneConfig(SettingsSaveOptions savePolicies, string filepath)
        {
            var saver = new ScriptedSceneSaver(savePolicies);
            var settings = saver.Save(_activeScene);
            _sceneStore.Add(settings);
            var json = _sceneStore.Save(settings.Name);
            File.WriteAllText(filepath, json);
        }

        public void Update(double delta)
        {
            if (!_sceneSetup)
            {
                return;
            }
            HandleSceneChanges();
            HandleGraphicsChanges();
            _activeScene.Update();
        }

        public void Draw(double delta)
        {
            if (!_sceneSetup)
            {
                return;
            }
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            _activeScene.Draw();
        }

        public void HandleSceneChanges()
        {
        }

        public void HandleGraphicsChanges()
        {
            GraphicsMessage msg = new();
            if(!_messenger.Take(ref msg))
            {
                return;
            }

            _graphicsSettings.WindowWidth = msg.ChangeSet.WindowWidth;
            _graphicsSettings.WindowHeight = msg.ChangeSet.WindowHeight;
            _graphicsSettings.WindowBorder = msg.ChangeSet.WindowBorder;
            _graphicsSettings.RefreshRate = msg.ChangeSet.RefreshRate;
            _graphicsSettings.VSync = msg.ChangeSet.VSync;

            _window.VSync = _graphicsSettings.VSync;
            _window.WindowBorder = _graphicsSettings.WindowBorder;
            _window.Size = new Vector2D<int>(_graphicsSettings.WindowWidth, _graphicsSettings.WindowHeight);
            _window.UpdatesPerSecond = _graphicsSettings.RefreshRate;
            _window.FramesPerSecond = _graphicsSettings.RefreshRate;

            if(_graphicsSettings.SkipOverlay != msg.ChangeSet.SkipOverlay)
            {
                // Toggle overlay.
            }
            _graphicsSettings.SkipOverlay = msg.ChangeSet.SkipOverlay;

            _gl.Viewport(_window.Size);
        }

        public void Dispose()
        {
        }

        public void Shutdown()
        {
            EngineLog.For<EngineApp>().Info("Shutting down Engine...");
            EngineLog.For<EngineApp>().Info("...Scene...");
            _activeScene.Shutdown();
            EngineLog.For<EngineApp>().Info("Scene shutdown.");
            if (!_window.IsClosing)
            {
                _window.Closing -= Shutdown;
                _window.Close();
                return;
            }
        }
    }
}