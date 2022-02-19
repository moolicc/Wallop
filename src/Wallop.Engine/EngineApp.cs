using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Wallop.DSLExtension.Scripting;
using Wallop.DSLExtension.Types.Plugin;
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
        private IWindow _window;

        private Settings.GraphicsSettings _graphicsSettings;
        private Settings.SceneSettings _sceneSettings;

        //private GraphicsDevice _graphicsDevice;
        private GL _gl;

        private bool _sceneSetup;

        private PluginPantry.PluginContext _pluginContext;

        private ScriptHostFunctions _scriptHostFunctions;
        private IEnumerable<IScriptEngineProvider> _scriptEngineProviders;
        private IEnumerable<KeyValuePair<string, Type>> _bindableComponentTypes;

        private Scene _scene;

        public EngineApp(Cog.Configuration config, PluginPantry.PluginContext pluginContext)
        {
            EngineLog.For<EngineApp>().Debug("Loading configurations...");
            _graphicsSettings = config.Get<Settings.GraphicsSettings>() ?? new Settings.GraphicsSettings();
            _sceneSettings = config.Get<Settings.SceneSettings>() ?? new Settings.SceneSettings();


            EngineLog.For<EngineApp>().Debug("Executing plugins on EngineStartup...");
            _pluginContext = pluginContext;
            _pluginContext.ExecuteEndPoint(new Types.Plugins.EngineStartupEndPoint { GraphicsSettings = _graphicsSettings });

            _sceneSetup = false;
            _scriptHostFunctions = new ScriptHostFunctions();
        }

        public void SetupWindow()
        {
            EngineLog.For<EngineApp>().Info("Creating window with options: {options}", _graphicsSettings);
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(_graphicsSettings.WindowWidth, _graphicsSettings.WindowHeight);
            options.WindowBorder = _graphicsSettings.WindowBorder;
            options.IsVisible = _graphicsSettings.SkipOverlay;
            options.FramesPerSecond = _graphicsSettings.RefreshRate;
            options.UpdatesPerSecond = _graphicsSettings.RefreshRate;
            options.VSync = _graphicsSettings.VSync;

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

            SetupScene();
        }

        private void WindowResized(Vector2D<int> size)
        {
            //_graphicsDevice.GetOpenGLInstance().Viewport(size);
            _gl.Viewport(size);
        }



        public void SetupScene()
        {
            EngineLog.For<EngineApp>().Info("Warming things up...");
            LoadModules();
            LoadScriptEngineProviders();
            LoadScene();

            _sceneSetup = true;
            EngineLog.For<EngineApp>().Info("Setup complete!");
        }

        private void LoadModules()
        {
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
            EngineLog.For<EngineApp>().Info("Loading scene settings...");
            _sceneSettings = new Settings.SceneSettings()
            {
                DirectorModules = new List<Settings.StoredModule>()
                {
                    new Settings.StoredModule()
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
                Layouts = new List<Settings.StoredLayout>()
                {
                    new Settings.StoredLayout()
                    {
                        Active = true,
                        Name = "layout1",
                        ActorModules = new List<Settings.StoredModule>()
                        {
                            new Settings.StoredModule()
                            {
                                InstanceName = "Square",
                                ModuleId = "Square.Test1.0",
                                Settings = new Dictionary<string, string>()
                                {
                                    { "height", "100" },
                                    { "width", "100" }
                                }
                            },
                            new Settings.StoredModule()
                            {
                                InstanceName = "Square1",
                                ModuleId = "Square.Test1.0",
                                Settings = new Dictionary<string, string>()
                                {
                                    { "height", "100" },
                                    { "width", "100" },
                                    { "y", "200" }
                                },
                                StoredBindings = new List<Settings.StoredBinding>()
                                {
                                    new Settings.StoredBinding()
                                    {
                                        PropertyName = "X",
                                        TypeName = "PositionComponent",
                                        SettingName = "x",
                                    },
                                    new Settings.StoredBinding()
                                    {
                                        PropertyName = "Y",
                                        TypeName = "PositionComponent",
                                        SettingName = "y",
                                    },
                                }
                            }
                        }
                    }
                }
            };


            EngineLog.For<EngineApp>().Info("Constructing scene...");
            var sceneLoader = new ScriptedSceneLoader(_sceneSettings);
            _scene = sceneLoader.LoadFromPackages(@"C:\Users\joel\source\repos\moolicc\Wallop\modules\squaretest");
            _scene.Init(_pluginContext);


            EngineLog.For<EngineApp>().Debug("Injecting script HostData...");
            var hostData = new HostData(_gl, _scene, _bindableComponentTypes);
            //_scriptHostFunctions.AddDependencyProperty(MemberNames.GET_NAME, hostData.GetName, null);
            _scriptHostFunctions.AddDependencies(hostData);



            EngineLog.For<EngineApp>().Info("Initializing scene and associated modules...");
            var initializer = new ScriptedSceneInitializer(_scriptHostFunctions, _scene, _pluginContext, _scriptEngineProviders, _bindableComponentTypes);
            initializer.InitializeActorScripts();
            initializer.InitializeDirectorScripts();
        }


        public void Update(double delta)
        {
            if(!_sceneSetup)
            {
                return;
            }
            _scene.Update();
        }

        public void Draw(double delta)
        {
            if (!_sceneSetup)
            {
                return;
            }
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            _scene.Draw();
        }

        public void Dispose()
        {
        }

        public void Shutdown()
        {
            EngineLog.For<EngineApp>().Info("Shutting down Engine...");
            EngineLog.For<EngineApp>().Info("...Scene...");
            _scene.Shutdown();
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