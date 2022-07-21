using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Wallop.Rendering
{
    public class GraphicsDevice
    {
        public GraphicsInformation Information { get; init; }

        private GL _oglInstance;
        
        public GraphicsDevice(GL underLyingGLInstance)
        {
            _oglInstance = underLyingGLInstance;

            Information = new GraphicsInformation(
                new Version(underLyingGLInstance.GetInteger(GLEnum.MajorVersion), underLyingGLInstance.GetInteger(GLEnum.MinorVersion)),
                underLyingGLInstance.GetInteger(GLEnum.UniformBufferOffsetAlignment),
                underLyingGLInstance.GetInteger(GLEnum.MaxUniformBufferBindings),
                underLyingGLInstance.GetInteger(GLEnum.MaxUniformBlockSize),
                underLyingGLInstance.GetInteger(GLEnum.MaxSamples),
                underLyingGLInstance.GetInteger(GLEnum.MaxTextureSize),
                underLyingGLInstance.GetInteger(GLEnum.MaxTextureImageUnits),
                underLyingGLInstance.GetInteger(GLEnum.MaxTextureBufferSize),
                underLyingGLInstance.GetInteger(GLEnum.Max3DTextureSize),
                underLyingGLInstance.GetInteger(GLEnum.MaxCubeMapTextureSize),
                underLyingGLInstance.GetInteger(GLEnum.MaxRectangleTextureSize),
                underLyingGLInstance.GetInteger(GLEnum.MaxRenderbufferSize),
                underLyingGLInstance.GetInteger(GLEnum.MaxVertexAttribs),
                underLyingGLInstance.GetInteger(GLEnum.MaxArrayTextureLayers),
                underLyingGLInstance.GetInteger(GLEnum.MaxColorAttachments),
                underLyingGLInstance.GetInteger(GLEnum.MaxDrawBuffers),
                underLyingGLInstance.GetInteger(GLEnum.MaxClipDistances),
                underLyingGLInstance.GetInteger(GLEnum.MaxTransformFeedbackBuffers),
                underLyingGLInstance.GetInteger(GLEnum.MaxTransformFeedbackInterleavedComponents),
                underLyingGLInstance.GetInteger(GLEnum.MaxTransformFeedbackSeparateComponents),
                underLyingGLInstance.GetInteger(GLEnum.MaxTransformFeedbackSeparateAttribs),
                underLyingGLInstance.GetInteger(GLEnum.MaxShaderStorageBufferBindings),
                underLyingGLInstance.GetInteger(GLEnum.MaxAtomicCounterBufferBindings),
                underLyingGLInstance.GetInteger(GLEnum.MaxFragmentUniformComponents),
                underLyingGLInstance.GetInteger(GLEnum.MaxUniformLocations),
                underLyingGLInstance.GetInteger(GLEnum.MaxVaryingComponents)
                );
        }

        public GL GetOpenGLInstance()
            => _oglInstance;

        internal void SetEffect(Effect effect)
            => SetEffect(effect.NativePointer);

        internal void ClearEffect()
            => SetEffect(0);

        public void SetEffect(uint effectProgram)
        {
            _oglInstance.UseProgram(effectProgram);
        }


        internal void SetBufferObject<TData>(BufferObject<TData> vbo) where TData : unmanaged
            => SetBufferObject(vbo.BufferType, vbo.NativePointer);

        internal void SetBufferObject(BufferTargetARB bufferType, uint buffer)
        {
            _oglInstance.BindBuffer(bufferType, buffer);
        }

        public void Clear(Color color)
            => Clear(color.R, color.G, color.B, color.A);

        public void Clear(float r, float g, float b, float a)
        {
            _oglInstance.ClearColor(r, g, b, a);
            _oglInstance.Clear(ClearBufferMask.ColorBufferBit);
        }

        public void Log(string? info)
            => Log(null, info);

        public void Log(string? context, string? info)
        {
            if(info == null || string.IsNullOrEmpty(info))
            {
                return;
            }

            if(context != null)
            {
                Console.WriteLine("[{0}] {1}", context, info);
            }
            else
            {
                Console.WriteLine("{0}", info);
            }
        }
    }
}
