using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Rendering
{
    public class Effect : GraphicsResource
    {
        public uint NativePointer { get; private set; }

        private Shader[] _shaders;

        public Effect(Shader[] shaders, string effectName)
            : base(effectName)
        {
            _shaders = shaders;
        }

        protected override void DeviceBound(GraphicsDevice device)
        {
            var gl = device.GetOpenGLInstance();
            NativePointer = gl.CreateProgram();

            foreach (var shader in _shaders)
            {
                gl.AttachShader(NativePointer, shader.NativePointer);
            }
            gl.LinkProgram(NativePointer);

            var logInfo = gl.GetShaderInfoLog(NativePointer);
            device.Log(ResourceName, logInfo);

            // TODO: Do we REALLY need to detach shaders RIGHT here?
            // https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Tutorials/Tutorial%201.2%20-%20Hello%20quad/Program.cs#L145

            Array.Clear(_shaders, 0, _shaders.Length);
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
