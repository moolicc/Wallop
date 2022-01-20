using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Rendering
{
    public class Shader : GraphicsResource
    {
        public uint NativePointer { get; private set; }

        public ShaderType Kind { get; private set; } 

        private string _source;

        public Shader(ShaderType shaderKind, string source)
        {
            Kind = shaderKind;
            _source = source;
        }

        protected override void DeviceBound(GraphicsDevice device)
        {
            var gl = device.GetOpenGLInstance();
            NativePointer = gl.CreateShader(Kind);
            
            gl.ShaderSource(NativePointer, _source);
            gl.CompileShader(NativePointer);

            var logInfo = gl.GetShaderInfoLog(NativePointer);
            device.Log($"{Kind}", logInfo);
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                var gl = GraphicsDevice?.GetOpenGLInstance();
                if(gl != null)
                {
                    gl.DeleteShader(NativePointer);
                }
            }
        }
    }
}
