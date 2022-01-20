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
using Wallop.Engine.Types.Plugins.EndPoints;


// TODO:
// Write a thread-safe rendering mechanism that can be shared by all resources contained in a Layout.
// Write a feature-rich set of HostApis in the plugin of the same name


namespace Wallop.Engine
{
    internal class EngineApp : IDisposable
    {
        private IWindow _window;

        private Settings.GraphicsSettings _graphicsSettings;
        private Settings.SceneSettings _sceneSettings;

        private GraphicsDevice _graphicsDevice;

        private PluginPantry.PluginContext _pluginContext;
        private ScriptedActorRunner _actorRunner;



        public EngineApp(Cog.Configuration config, PluginPantry.PluginContext pluginContext)
        {
            _graphicsSettings = config.Get<Settings.GraphicsSettings>() ?? new Settings.GraphicsSettings();
            _sceneSettings = config.Get<Settings.SceneSettings>() ?? new Settings.SceneSettings();
            _pluginContext = pluginContext;
            _pluginContext.ExecuteEndPoint(new Types.Plugins.EngineStartupEndPoint { GraphicsSettings = _graphicsSettings });
        }

        public void Setup()
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(_graphicsSettings.WindowWidth, _graphicsSettings.WindowHeight);
            options.WindowBorder = _graphicsSettings.WindowBorder;
            options.IsVisible = _graphicsSettings.SkipOverlay;
            options.FramesPerSecond = _graphicsSettings.RefreshRate;
            options.UpdatesPerSecond = _graphicsSettings.RefreshRate;
            options.VSync = _graphicsSettings.VSync;

            _window = Window.Create(options);
            _window.Load += WindowLoad;
            _window.FramebufferResize += WindowResized;
            _window.Update += Update;
            _window.Render += Draw;
            _window.Closing += Shutdown;

            _window.Run();
        }

        private void WindowLoad()
        {
            _graphicsDevice = new GraphicsDevice(_window.CreateOpenGL());

            if (!_graphicsSettings.SkipOverlay)
            {
                _pluginContext.ExecuteEndPoint(new OverlayerEndPoint(_window));
                _pluginContext.WaitForEndPointExecutionAsync<OverlayerEndPoint>().ContinueWith(_ =>
                {
                    _window.IsVisible = true;
                });
            }

            LoadModules();
        }

        private void WindowResized(Vector2D<int> size)
        {
            _graphicsDevice.GetOpenGLInstance().Viewport(size);
        }

        private void LoadModules()
        {
            var engineEndPointPluginContext = new ScriptEngineEndPoint();
            _pluginContext.ExecuteEndPoint<ILoadingScriptEnginesEndPoint>(engineEndPointPluginContext);
            _pluginContext.WaitForEndPointExecutionAsync<ILoadingScriptEnginesEndPoint>().WaitAndThrow();

            _sceneSettings = new Settings.SceneSettings()
            {
                DirectorModules = new List<Settings.StoredModule>(),
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
                            }
                        }
                    }
                }
            };
            var sceneLoader = new ScriptedSceneLoader(_sceneSettings);
            var scene = sceneLoader.LoadFromPackages(@"C:\Users\joel\source\repos\moolicc\Wallop\modules\squaretest");

            var initializer = new ScriptedSceneInitializer(scene, _pluginContext);
            _actorRunner = new ScriptedActorRunner(engineEndPointPluginContext.GetScriptEngineProviders());
            initializer.InitializeScripts(_actorRunner);
        }


        public void Update(double delta)
        {
            _actorRunner.InvokeUpdate();
            _actorRunner.WaitAsync().WaitAndThrow();
        }

        public void Draw(double delta)
        {
            _actorRunner.InvokeRender();
            _actorRunner.WaitAsync().WaitAndThrow();
        }

        public void Dispose()
        {
        }

        public void Shutdown()
        {
            if(!_window.IsClosing)
            {
                _window.Close();
                return;
            }
        }
    }
}