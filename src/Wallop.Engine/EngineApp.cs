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
            var scene = sceneLoader.LoadFromPackages(@"C:\Users\joel\source\repos\moolicc\Wallop\modules\squaretest");

            var initializer = new ScriptedSceneInitializer(scene, _pluginContext);
            _actorRunner = new ScriptedActorRunner(engineEndPointPluginContext.GetScriptEngineProviders());

            //initializer.GLInstance = _graphicsDevice.GetOpenGLInstance();
            initializer.GLInstance = _gl;

            initializer.InitializeScripts(_actorRunner);
        }


        public void Update(double delta)
        {
            _actorRunner.Invoke<ScriptedActor>(ScriptContextExtensions.VariableNames.UPDATE, false, BeforeActorUpdate, AfterActorUpdate);
            _actorRunner.WaitAsync().WaitAndThrow();
        }

        public void Draw(double delta)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            _actorRunner.Invoke<ScriptedActor>(ScriptContextExtensions.VariableNames.DRAW, false, BeforeActorDraw, AfterActorDraw);
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

        // TODO: We need to cache apis on actors.
        private void BeforeActorUpdate(ScriptedActor actor)
        {
            var engine = actor.ScriptEngine.OrThrow("Actor does not have a ScriptEngine bound.");
            var context = engine.GetAttachedScriptContext().OrThrow("ScriptEngine does not have a context bound.");

            var apis = _pluginContext.GetImplementations<IHostApi>();
            foreach (var targetApi in actor.ControllingModule.ModuleInfo.HostApis)
            {
                var api = apis.FirstOrDefault(a => a.Name == targetApi);
                if(api != null)
                {
                    api.BeforeUpdate(context, 0.0);
                }
            }
        }

        private void AfterActorUpdate(ScriptedActor actor)
        {
            var engine = actor.ScriptEngine.OrThrow("Actor does not have a ScriptEngine bound.");
            var context = engine.GetAttachedScriptContext().OrThrow("ScriptEngine does not have a context bound.");

            var apis = _pluginContext.GetImplementations<IHostApi>();
            foreach (var targetApi in actor.ControllingModule.ModuleInfo.HostApis)
            {
                var api = apis.FirstOrDefault(a => a.Name == targetApi);
                if (api != null)
                {
                    api.AfterUpdate(context);
                }
            }
        }

        private void BeforeActorDraw(ScriptedActor actor)
        {
            var engine = actor.ScriptEngine.OrThrow("Actor does not have a ScriptEngine bound.");
            var context = engine.GetAttachedScriptContext().OrThrow("ScriptEngine does not have a context bound.");

            var apis = _pluginContext.GetImplementations<IHostApi>();
            foreach (var targetApi in actor.ControllingModule.ModuleInfo.HostApis)
            {
                var api = apis.FirstOrDefault(a => a.Name == targetApi);
                if (api != null)
                {
                    api.BeforeDraw(context, 0.0);
                }
            }
        }

        private void AfterActorDraw(ScriptedActor actor)
        {
            var engine = actor.ScriptEngine.OrThrow("Actor does not have a ScriptEngine bound.");
            var context = engine.GetAttachedScriptContext().OrThrow("ScriptEngine does not have a context bound.");

            var apis = _pluginContext.GetImplementations<IHostApi>();
            foreach (var targetApi in actor.ControllingModule.ModuleInfo.HostApis)
            {
                var api = apis.FirstOrDefault(a => a.Name == targetApi);
                if (api != null)
                {
                    api.AfterDraw(context);
                }
            }
        }
    }
}