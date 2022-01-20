using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using Wallop.Engine.Settings;

namespace Wallop.Engine.Rendering
{
    public class GraphicsManager
    {
        public bool RenderingEnabled { get; set; }
        public Sdl2Window Window { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }
        public ResourceFactory Resources { get; private set;}


        private GraphicsSettings _graphicsSettings;


        public GraphicsManager(GraphicsSettings settings)
        {
            _graphicsSettings = settings;
        }

        public void Setup()
        {
            SetupWindowAndGraphicsDevice();
            SetupGraphicsResources();
        }

        public void EnableRenderingAndShowWindow()
        {
            RenderingEnabled = true;
            Window.WindowState = WindowState.Normal;
        }

        private void SetupWindowAndGraphicsDevice()
        {
            var windowCi = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowHeight = _graphicsSettings.WindowWidth,
                WindowWidth = _graphicsSettings.WindowHeight,
                WindowTitle = "Wallop - Engine",
                
                WindowInitialState = _graphicsSettings.SkipOverlay ? WindowState.Normal : WindowState.Hidden,
            };

            var gfxOptions = new GraphicsDeviceOptions()
            {
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true,
                Debug = true,
            };

            Window = VeldridStartup.CreateWindow(ref windowCi);
            GraphicsDevice = VeldridStartup.CreateGraphicsDevice(Window, gfxOptions, _graphicsSettings.Backend);

            Window.BorderVisible = _graphicsSettings.WindowBorder;
            Window.Resized += WindowResized;

        }

        private void SetupGraphicsResources()
        {
            Resources = GraphicsDevice.ResourceFactory;
            DefaultShaders.CompileShaders(Resources);
            DefaultPipelines.Create(Resources, GraphicsDevice);
        }

        private void WindowResized()
        {
            GraphicsDevice.MainSwapchain.Resize((uint)Window.Width, (uint)Window.Height);
        }

        internal void Present()
        {
            GraphicsDevice.SwapBuffers();
        }
    }

    internal static class DefaultPipelines
    {
        public static Pipeline BasicPositionColor_Pipeline { get; private set; }

        public static void Create(ResourceFactory resourceFactory, GraphicsDevice graphicsDevice)
        {
            var basicDescription = CreateBasicPositionColorPipelineDesc(graphicsDevice.SwapchainFramebuffer.OutputDescription);
            BasicPositionColor_Pipeline = resourceFactory.CreateGraphicsPipeline(basicDescription);
        }

        private static GraphicsPipelineDescription CreateBasicPositionColorPipelineDesc(OutputDescription output)
        {
            var vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();

            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;

            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);

            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;

            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: DefaultShaders.BasicPositionColor_Shaders);

            pipelineDescription.Outputs = output;

            return pipelineDescription;
        }

        private static GraphicsPipelineDescription CreateBasicPositionTextPipelineDesc()
        {
            var vertexLayout = VertexPositionColor.LayoutDescription;

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();

            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;

            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);

            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Wireframe,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);

            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            pipelineDescription.ResourceLayouts = Array.Empty<ResourceLayout>();

            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: DefaultShaders.BasicPositionColor_Shaders);

            return pipelineDescription;
        }
    }

    internal static class DefaultShaders
    {
        public const string BASIC_POSITIONCOLOR_VERTEX_CODE = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;

layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(Position, 0, 1);
    fsin_Color = Color;
}";

        public const string BASIC_POSITONCOLOR_FRAGMENT_CODE = @"
#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";




        public static string DefaultPrimitiveVS = @"
            #version 450
            layout(set = 0, binding = 0) uniform ProjectionViewBuffer2 {
                mat4x4 uProjection;
                mat4x4 uView;
            };
            layout (location = 0) in vec2 vPosition;
            layout (location = 1) in vec2 vPadding;
            layout (location = 2) in vec4 vColor;
            layout (location = 1) out vec4 fColor;
            void main()
            {
                fColor = vColor;
                gl_Position = uProjection * uView * vec4(vPosition.x, vPosition.y, 0.0, 1.0);
            }
        ";

        public static string DefaultPrimitiveFS = @"
            #version 450
            layout (location = 1) in vec4 fColor;
            layout (location = 0) out vec4 fFragColor;
            void main()
            {
                fFragColor = fColor;
            }
        ";



        public static Shader[] BasicPositionColor_Shaders { get; private set; }

        public static void CompileShaders(ResourceFactory resourceFactory)
        {
            var vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(BASIC_POSITONCOLOR_FRAGMENT_CODE), "main");
            var fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(BASIC_POSITONCOLOR_FRAGMENT_CODE), "main");

            BasicPositionColor_Shaders = resourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
        }
    }
}
