using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
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
        private Settings.GraphicsSettings _graphicsSettings;
        private Settings.SceneSettings _sceneSettings;

        private PluginPantry.PluginContext _pluginContext;
        private Rendering.GraphicsManager _graphicsManager;
        private ScriptedActorRunner? _actorRunner;




        private Pipeline _pipeline;
        private DeviceBuffer _vertexBuffer;
        private CommandList _commandList;
        private VertexPositionColor[] _verts;


        public EngineApp(Cog.Configuration config, PluginPantry.PluginContext pluginContext)
        {
            _graphicsSettings = config.Get<Settings.GraphicsSettings>() ?? new Settings.GraphicsSettings();
            _sceneSettings = config.Get<Settings.SceneSettings>() ?? new Settings.SceneSettings();

            _pluginContext = pluginContext;
            _pluginContext.ExecuteEndPoint(new Types.Plugins.EngineStartupEndPoint { GraphicsSettings = _graphicsSettings });

            _graphicsManager = new GraphicsManager(_graphicsSettings);
        }

        public void Setup()
        {
            _graphicsManager.Setup();
            if(_graphicsSettings.SkipOverlay)
            {
                _graphicsManager.EnableRenderingAndShowWindow();
            }
            else
            {
                _pluginContext.ExecuteEndPoint(new OverlayerEndPoint(_graphicsManager.Window));
                _pluginContext.WaitForEndPointExecutionAsync<OverlayerEndPoint>().ContinueWith(_ => _graphicsManager.EnableRenderingAndShowWindow());
            }
            LoadModules();


            _vertexBuffer = _graphicsManager.Resources.CreateBuffer(new BufferDescription { Usage = BufferUsage.VertexBuffer, SizeInBytes = 6 * VertexPositionColor.SIZE });
            
            _verts = new VertexPositionColor[6];
            FillRect(ref _verts, new Rectangle(0, 0, 100, 100), RgbaFloat.Red);
            _graphicsManager.GraphicsDevice.UpdateBuffer(_vertexBuffer, 0, _verts);

            _pipeline = DefaultPipelines.BasicPositionColor_Pipeline;
            _commandList = _graphicsManager.Resources.CreateCommandList();
        }

        private void FillRect(ref VertexPositionColor[] data, Rectangle bounds, RgbaFloat color)
        {
            data[0] = new VertexPositionColor(bounds.TopLeft(), color);
            data[1] = new VertexPositionColor(bounds.TopRight(), color);
            data[2] = new VertexPositionColor(bounds.BottomLeft(), color);

            data[3] = new VertexPositionColor(bounds.BottomRight(), color);
            data[4] = new VertexPositionColor(bounds.TopRight(), color);
            data[5] = new VertexPositionColor(bounds.BottomLeft(), color);
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

        public void Run()
        {
            while(_graphicsManager.Window.Exists)
            {
                _graphicsManager.Window.PumpEvents();
                Update();
                Draw();
            }
        }

        public void Update()
        {
            if(!_graphicsManager.RenderingEnabled)
            {
                return;
            }
            //_actorRunner.InvokeUpdate();
            //_actorRunner.WaitAsync().WaitAndThrow();
        }

        public void Draw()
        {
            if (!_graphicsManager.RenderingEnabled)
            {
                return;
            }

            _commandList.Begin();
            _commandList.SetFramebuffer(_graphicsManager.GraphicsDevice.SwapchainFramebuffer);
            _commandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetPipeline(_pipeline);
            _commandList.DrawIndexed(6);
            _commandList.End();


            _graphicsManager.GraphicsDevice.SubmitCommands(_commandList);
            _graphicsManager.GraphicsDevice.SwapBuffers();
            //_actorRunner.InvokeRender();
            //_actorRunner.WaitAsync().WaitAndThrow();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPositionColor
{
    public const uint SIZE = 24;

    public static readonly VertexLayoutDescription LayoutDescription = new VertexLayoutDescription(
        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
        new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
    );

    public Vector2 Position;
    public RgbaFloat Color;

    public VertexPositionColor(Vector2 position, RgbaFloat color)
        : this()
    {
        Position = position;
        Color = color;
    }

}