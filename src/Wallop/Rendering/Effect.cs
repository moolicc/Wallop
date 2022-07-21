using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Rendering
{
    internal class Effect : GraphicsResource
    {
        public uint NativePointer { get; private set; }

        private Shader[]? _shaders;

        public Effect(Shader[] shaders, string effectName)
            : base(effectName)
        {
            _shaders = shaders;
        }


        public bool GetIsActive()
        {
            if (GraphicsDevice == null)
            {
                throw new ArgumentNullException(nameof(GraphicsDevice), "GraphicsDevice not bound!");
            }
            if (NativePointer == 0)
            {
                throw new ArgumentNullException(nameof(NativePointer), "Shader program has no ID!");
            }

            var program = GraphicsDevice.GetOpenGLInstance().GetInteger(Silk.NET.OpenGL.GLEnum.CurrentProgram);
            return program == NativePointer;
        }

        public void Begin()
        {
            if (GraphicsDevice == null)
            {
                throw new ArgumentNullException(nameof(GraphicsDevice), "GraphicsDevice not bound!");
            }
            if (NativePointer == 0)
            {
                throw new ArgumentNullException(nameof(NativePointer), "Shader program has no ID!");
            }
            GraphicsDevice.GetOpenGLInstance().UseProgram(NativePointer);
        }

        public void End()
        {
            if (GraphicsDevice == null)
            {
                throw new NullReferenceException("GraphicsDevice not bound!");
            }
            if (NativePointer == 0)
            {
                throw new NullReferenceException("Shader program has no ID!");
            }
            GraphicsDevice.GetOpenGLInstance().UseProgram(0);
        }

        protected override void DeviceBound(GraphicsDevice device)
        {
            if(_shaders == null)
            {
                throw new NullReferenceException("Effect created without shaders being set.");
            }

            var gl = device.GetOpenGLInstance();
            NativePointer = gl.CreateProgram();

            foreach (var shader in _shaders)
            {
                gl.AttachShader(NativePointer, shader.NativePointer);
            }
            gl.LinkProgram(NativePointer);

            var logInfo = gl.GetProgramInfoLog(NativePointer);
            device.Log(ResourceName, logInfo);

            logInfo = gl.GetShaderInfoLog(NativePointer);
            device.Log(ResourceName, logInfo);

            // TODO: Do we REALLY need to detach shaders RIGHT here?
            // https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Tutorials/Tutorial%201.2%20-%20Hello%20quad/Program.cs#L145

            _shaders = null;
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                var gl = GraphicsDevice?.GetOpenGLInstance();
                if(gl != null)
                {
                    gl.DeleteProgram(NativePointer);
                }
            }
        }
    }
}
