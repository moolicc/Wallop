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

        private PluginPantry.PluginContext _pluginContext;

        private Scene _scene;

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

            Window.PrioritizeSdl();
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
            //_graphicsDevice = new GraphicsDevice(GL.GetApi(_window));
            //var gl = _graphicsDevice.Information;
            _gl = GL.GetApi(_window);




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
            //_graphicsDevice.GetOpenGLInstance().Viewport(size);
            _gl.Viewport(size);
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
            _scene = sceneLoader.LoadFromPackages(@"C:\Users\joel\source\repos\moolicc\Wallop\modules\squaretest");
            _scene.Init(_pluginContext);

            var initializer = new ScriptedSceneInitializer(_gl, _scene, _pluginContext, engineEndPointPluginContext.GetScriptEngineProviders());

            initializer.InitializeActorScripts();
            initializer.InitializeDirectorScripts();
        }


        public void Update(double delta)
        {
            _scene.Update();
        }

        public void Draw(double delta)
        {
            Console.WriteLine("Draw Delta: {0}", delta);
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            _scene.Draw();
        }

        public void Dispose()
        {
        }

        public void Shutdown()
        {
            _scene.Shutdown();
            if(!_window.IsClosing)
            {
                _window.Closing -= Shutdown;
                _window.Close();
                return;
            }
        }
    }
}