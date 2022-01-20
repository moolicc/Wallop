using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.SPIRV;

namespace Wallop.Engine.Rendering
{
    public enum VertexTemplateType
    {
        TopLeft = 0,
        TopRight,
        BottomLeft,
        BottomRight
    }

    /*
    public class PrimitiveBatch : IDisposable
    {
        public static GraphicsDevice GraphicsDevice { get; set; }
        public static CommandList CommandList { get; set; }

        // Shared static resources
        protected static bool _staticResLoaded = false;
        protected static Shader[] _shaders;
        public static Sampler _sampler;

        protected static Vector2[] _rectVertexTemplate = new Vector2[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
        };

        // Rendering
        protected Matrix4x4 _projection;
        protected Matrix4x4 _view;

        // Graphics resources
        protected Pipeline _currentPipeline;
        protected Pipeline _pipelineTriangleList;
        protected Pipeline _pipelineTriangleStrip;
        protected Pipeline _pipelineLineLoop;
        protected DeviceBuffer _vertexBuffer;
        protected DeviceBuffer _transformBuffer;
        protected ResourceLayout _transformLayout;
        protected ResourceSet _transformSet;

        protected const int _maxBatchSize = 50000;
        protected bool _begin = false;
        protected Vertex2DPositionColor[] _vertexData;
        protected Vertex2DPositionColor[] _tempVertices;
        protected int _currentBatchCount = 0;

        #region IDisposable
        protected bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _pipelineTriangleList?.Dispose();
                    _pipelineTriangleStrip?.Dispose();
                    _pipelineLineLoop?.Dispose();
                    _vertexBuffer?.Dispose();
                    _transformBuffer?.Dispose();
                    _transformLayout?.Dispose();
                    _transformSet?.Dispose();
                }

                _disposed = true;
            }
        }
        #endregion

        public unsafe PrimitiveBatch(int width, int height, OutputDescription output, bool invertY = false)
        {
            var factory = GraphicsDevice.ResourceFactory;
            LoadStaticResources(factory);

            _projection = Matrix4x4.CreateOrthographicOffCenter(0f, width, height, 0f, 0f, 1f);

            if (invertY && !GraphicsDevice.IsUvOriginTopLeft)
                _projection = Matrix4x4.CreateOrthographicOffCenter(0f, width, 0f, height, 0f, 1f);

            _transformBuffer = factory.CreateBuffer(new BufferDescription((uint)(sizeof(Matrix4x4) * 2), BufferUsage.UniformBuffer));
            GraphicsDevice.UpdateBuffer(_transformBuffer, 0, Matrix4x4.Identity);
            GraphicsDevice.UpdateBuffer(_transformBuffer, (uint)sizeof(Matrix4x4), Matrix4x4.Identity);

            _transformLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(new ResourceLayoutElementDescription("ProjectionViewBuffer2", ResourceKind.UniformBuffer, ShaderStages.Vertex)));
            _transformSet = factory.CreateResourceSet(new ResourceSetDescription(_transformLayout, _transformBuffer));

            _vertexData = new Vertex2DPositionColor[_maxBatchSize];
            _tempVertices = new Vertex2DPositionColor[_maxBatchSize];

            _vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(_vertexData.Length * sizeof(Vertex2DPositionColor)), BufferUsage.VertexBuffer));
            GraphicsDevice.UpdateBuffer(_vertexBuffer, 0, ref _vertexData[0], (uint)(_vertexData.Length * sizeof(Vertex2DPositionColor)));

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleAlphaBlend,
                DepthStencilState = new DepthStencilStateDescription(depthTestEnabled: true, depthWriteEnabled: true, ComparisonKind.LessEqual),
                RasterizerState = new RasterizerStateDescription
                {
                    DepthClipEnabled = true,
                    CullMode = FaceCullMode.None,
                },
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ShaderSet = new ShaderSetDescription(vertexLayouts: new VertexLayoutDescription[] { Vertex2DPositionColor.VertexLayout }, _shaders),
                ResourceLayouts = new ResourceLayout[]
                {
                    _transformLayout
                },
                Outputs = output
            };

            _pipelineTriangleList = factory.CreateGraphicsPipeline(pipelineDescription);

            pipelineDescription.PrimitiveTopology = PrimitiveTopology.LineStrip;
            _pipelineLineLoop = factory.CreateGraphicsPipeline(pipelineDescription);

            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            _pipelineTriangleStrip = factory.CreateGraphicsPipeline(pipelineDescription);
        }

        public static void LoadStaticResources(ResourceFactory factory)
        {
            if (_staticResLoaded)
                return;

            var vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(DefaultShaders.DefaultPrimitiveVS), "main");
            var fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(DefaultShaders.DefaultPrimitiveFS), "main");

            _shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc, new CrossCompileOptions(fixClipSpaceZ: true, invertVertexOutputY: false));
            _staticResLoaded = true;
        }

        public void Begin(Matrix4x4? view = null)
        {
            Begin(GraphicsDevice.PointSampler, view);

        }

        public void DrawOutlinedRect(Rectangle rect, RgbaFloat color, RgbaFloat lineColor, int lineSize, Vector2? origin = null, float rotation = 0f)
        {
            DrawFilledRect(rect, color, origin, rotation);
            DrawEmptyRect(rect, lineColor, lineSize, origin, rotation);
        }

        public void DrawEmptyRect(Rectangle rect, RgbaFloat color, int lineSize, Vector2? origin = null, float rotation = 0f)
        {
            if (!origin.HasValue)
                origin = new Vector2(0f, 0f);

            var topRect = new Rectangle(rect.X, rect.Y, rect.Width, lineSize);
            var bottomRect = new Rectangle(rect.X, rect.Y + (rect.Height - lineSize), rect.Width, lineSize);
            var leftRect = new Rectangle(rect.X, rect.Y + lineSize, lineSize, (rect.Height - lineSize * 2));
            var rightRect = new Rectangle(rect.X + (rect.Width - lineSize), rect.Y + lineSize, lineSize, (rect.Height - lineSize * 2));

            // top
            DrawFilledRect(topRect, color, origin, rotation);
            // bottom
            DrawFilledRect(bottomRect, color, origin, rotation);
            // left
            DrawFilledRect(leftRect, color, origin, rotation);
            // right
            DrawFilledRect(rightRect, color, origin, rotation);
        }

        public void DrawFilledRect(Rectangle rect, RgbaFloat color, Vector2? origin = null, float rotation = 0f)
        {
            if (!origin.HasValue)
                origin = new Vector2(0f, 0f);

            var sin = 0f;
            var cos = 0f;
            var nOriginX = -origin.Value.X;
            var nOriginY = -origin.Value.Y;

            if (rotation != 0f)
            {
                var radians = 0.0f;//rotation.ToRadians();
                sin = MathF.Sin(radians);
                cos = MathF.Cos(radians);
            }

            var topLeft = new Vertex2DPositionColor()
            {
                Position = rotation == 0.0f
                            ? new Vector2(
                                rect.X - origin.Value.X,
                                rect.Y - origin.Value.Y)
                            : new Vector2(
                                rect.X + nOriginX * cos - nOriginY * sin,
                                rect.Y + nOriginX * sin + nOriginY * cos),
                Color = color
            };

            var x = _rectVertexTemplate[(int)VertexTemplateType.TopRight].X;
            var w = rect.Width * x;

            var topRight = new Vertex2DPositionColor()
            {
                Position = rotation == 0.0f
                            ? new Vector2(
                                (rect.X - origin.Value.X) + w,
                                rect.Y - origin.Value.Y)
                            : new Vector2(
                                rect.X + (nOriginX + w) * cos - nOriginY * sin,
                                rect.Y + (nOriginX + w) * sin + nOriginY * cos),
                Color = color
            };

            var y = _rectVertexTemplate[(int)VertexTemplateType.BottomLeft].Y;
            var h = rect.Height * y;

            var bottomLeft = new Vertex2DPositionColor()
            {
                Position = rotation == 0.0f
                            ? new Vector2(
                                (rect.X - origin.Value.X),
                                (rect.Y - origin.Value.Y) + h)
                            : new Vector2(
                                rect.X + nOriginX * cos - (nOriginY + h) * sin,
                                rect.Y + nOriginX * sin + (nOriginY + h) * cos),
                Color = color
            };

            x = _rectVertexTemplate[(int)VertexTemplateType.BottomRight].X;
            y = _rectVertexTemplate[(int)VertexTemplateType.BottomRight].Y;
            w = rect.Width * x;
            h = rect.Height * y;

            var bottomRight = new Vertex2DPositionColor()
            {
                Position = rotation == 0.0f
                            ? new Vector2(
                                (rect.X - origin.Value.X) + w,
                                (rect.Y - origin.Value.Y) + h)
                            : new Vector2(
                                rect.X + (nOriginX + w) * cos - (nOriginY + h) * sin,
                                rect.Y + (nOriginX + w) * sin + (nOriginY + h) * cos),
                Color = color
            };

            _tempVertices[0] = bottomLeft;
            _tempVertices[1] = topRight;
            _tempVertices[2] = topLeft;
            _tempVertices[3] = bottomLeft;
            _tempVertices[4] = bottomRight;
            _tempVertices[5] = topRight;

            AddVertices(_pipelineTriangleList, 6);
        }

        public void DrawOutlinedCircle(Vector2 position, float radius, RgbaFloat color, RgbaFloat lineColor, int quality = 10)
        {
            DrawFilledCircle(position, radius, color, quality);
            DrawEmptyCircle(position, radius, lineColor, quality);
        }

        public void DrawEmptyCircle(Vector2 position, float radius, RgbaFloat color, int quality = 10)
        {
            var vertexCount = (int)(radius / quality) * 8;
            var loopIndex = 0;

            for (var i = 0f; i < 2 * MathF.PI; i += 2 * MathF.PI / vertexCount)
            {
                _tempVertices[loopIndex] = new Vertex2DPositionColor()
                {
                    Position = new Vector2(MathF.Cos(i) * radius + position.X, MathF.Sin(i) * radius + position.Y),
                    Color = color,
                };

                loopIndex += 1;
            }

            _tempVertices[loopIndex] = new Vertex2DPositionColor()
            {
                Position = _tempVertices[0].Position,
                Color = color,
            };

            loopIndex += 1;

            AddVertices(_pipelineLineLoop, loopIndex);
        }

        public void DrawFilledCircle(Vector2 position, float radius, RgbaFloat color, int quality = 10)
        {
            var vertexCount = (int)(radius / quality) * 8;
            var loopIndex = 0;

            for (var i = 0f; i < 2 * MathF.PI; i += 2 * MathF.PI / vertexCount)
            {
                _tempVertices[loopIndex] = new Vertex2DPositionColor()
                {
                    Position = new Vector2(MathF.Cos(i) * radius + position.X, MathF.Sin(i) * radius + position.Y),
                    Color = color,
                };

                loopIndex += 1;

                _tempVertices[loopIndex] = new Vertex2DPositionColor()
                {
                    Position = position,
                    Color = color,
                };

                loopIndex += 1;
            }

            _tempVertices[loopIndex] = new Vertex2DPositionColor()
            {
                Position = _tempVertices[0].Position,
                Color = color,
            };

            loopIndex += 1;

            AddVertices(_pipelineTriangleStrip, loopIndex);
        }

        public unsafe void Begin(Sampler sampler, Matrix4x4? view = null)
        {
            if (_begin)
                throw new Exception("You must end the current batch before starting a new one.");

            if (view == null)
                _view = Matrix4x4.Identity;
            else
                _view = view.Value;

            _sampler = sampler;
            _begin = true;

            CommandList.UpdateBuffer(_transformBuffer, 0, _projection);
            CommandList.UpdateBuffer(_transformBuffer, (uint)sizeof(Matrix4x4), _view);
        }

        protected void AddVertices(Pipeline pipeline, int count)
        {
            if (!_begin)
                throw new Exception("You must begin a batch before you can call Draw.");

            if (_currentPipeline == null || _currentPipeline != pipeline)
            {
                if (_currentPipeline != null)
                    Flush();

                _currentPipeline = pipeline;
            }

            if (_currentBatchCount + count >= _vertexData.Length)
                Flush();

            for (var i = 0; i < count; i++)
            {
                _vertexData[_currentBatchCount] = _tempVertices[i];
                _currentBatchCount += 1;
            }
        }

        public void End()
        {
            if (!_begin)
                throw new Exception("You must begin a batch before you can call End.");

            Flush();
            _begin = false;
        }

        public unsafe void Flush()
        {
            if (_currentBatchCount == 0 || _currentPipeline == null)
                return;

            CommandList.UpdateBuffer(_vertexBuffer, 0, ref _vertexData[0], (uint)(_currentBatchCount * sizeof(Vertex2DPositionColor)));
            CommandList.SetVertexBuffer(0, _vertexBuffer);
            CommandList.SetPipeline(_currentPipeline);
            CommandList.SetGraphicsResourceSet(0, _transformSet);
            CommandList.Draw((uint)_currentBatchCount);

            _currentBatchCount = 0;
            _currentPipeline = null;
        }

    }


    */

    /*
    public class PrimitiveBatch
    {
        private const int MAX_BATCH_SIZE = 1000;
        private const int MAX_VERTS_PER_RENDER = 1000;

        public RenderContext RenderContext { get; init; }


        private DeviceBuffer _vertexBuffer;

        private VertexPositionColor[] _batch;
        private VertexPositionColor[] _buffer;
        private int _curVert;

        public PrimitiveBatch(RenderContext context)
        {
            RenderContext = context;
            _batch = new VertexPositionColor[MAX_BATCH_SIZE];
            _buffer = new VertexPositionColor[MAX_VERTS_PER_RENDER];
            _curVert = 0;


            var factory = RenderContext.Device.ResourceFactory;

            var bufferDesc = new BufferDescription()
            {
                Usage = BufferUsage.VertexBuffer,
                SizeInBytes = MAX_BATCH_SIZE * VertexPositionColor.SIZE,
            };
            _vertexBuffer = factory.CreateBuffer(bufferDesc);
        }

        public void FillRectangle(Rectangle bounds, RgbaFloat color)
        {
            _buffer[0] = new VertexPositionColor(bounds.BottomLeft(), color);
            _buffer[1] = new VertexPositionColor(bounds.TopRight(), color);
            _buffer[2] = new VertexPositionColor(bounds.TopLeft(), color);

            _buffer[3] = new VertexPositionColor(bounds.BottomLeft(), color);
            _buffer[4] = new VertexPositionColor(bounds.BottomRight(), color);
            _buffer[5] = new VertexPositionColor(bounds.TopRight(), color);
            AddBatchFromBuffer(6);
            Flush();
        }

        public void FillRectangle(Vector2 position, Vector2 size, RgbaFloat color)
        {
            Vector2 topLeft = new Vector2(position.X, position.Y);
            Vector2 topRight = new Vector2(position.X + size.X, position.Y);
            Vector2 bottomLeft = new Vector2(position.X, position.Y + size.Y);
            Vector2 bottomRight = new Vector2(position.X + size.X, position.Y + size.Y);

            //_buffer[0] = new VertexPositionColor(bottomLeft, color);
            //_buffer[1] = new VertexPositionColor(topRight, color);
            //_buffer[2] = new VertexPositionColor(topLeft, color);

            //_buffer[3] = new VertexPositionColor(bottomLeft, color);
            //_buffer[4] = new VertexPositionColor(bottomRight, color);
            //_buffer[5] = new VertexPositionColor(topRight, color);

            _buffer[0] = new VertexPositionColor(topLeft, color);
            _buffer[1] = new VertexPositionColor(topRight, color);
            _buffer[2] = new VertexPositionColor(bottomRight, color);

            _buffer[3] = new VertexPositionColor(bottomRight, color);
            _buffer[4] = new VertexPositionColor(bottomLeft, color);
            _buffer[5] = new VertexPositionColor(topLeft, color);
            AddBatchFromBuffer(6);
            Flush();
        }

        public void Flush()
        {
            // TODO: Figure out instancing.


            RenderContext.Commands.UpdateBuffer(_vertexBuffer, 0, ref _batch[0], (uint)_curVert * VertexPositionColor.SIZE);
            RenderContext.Commands.SetVertexBuffer(0, _vertexBuffer);
            RenderContext.Commands.Draw((uint)_curVert);

            _curVert = 0;
        }

        private void AddBatchFromBuffer(int vertexCount)
        {
            for (int i = 0; i < vertexCount; i++)
            {
                _batch[i + _curVert] = _buffer[i];
            }
            _curVert += vertexCount;
        }
        
    }
    */
}
