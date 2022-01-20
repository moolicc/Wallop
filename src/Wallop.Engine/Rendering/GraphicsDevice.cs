using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;

namespace Wallop.Engine.Rendering
{
    public class GraphicsDevice
    {
        public GraphicsInformation Information { get; init; }

        private GL _oglInstance;
        
        public GraphicsDevice(GL underLyingGLInstance)
        {
            _oglInstance = underLyingGLInstance;
            _graphicsResources = new List<GraphicsResource>();

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
