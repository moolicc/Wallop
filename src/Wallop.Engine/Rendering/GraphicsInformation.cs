using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Rendering
{
    public record GraphicsInformation(
        Version OpenGLVersion, int UniformBufferOffsetAlignment, int MaxUniformBufferBindings, int MaxUniformBlockSize, int MaxSamples,
        int MaxTextureSize, int MaxTextureImageUnits, int MaxTextureBufferSize, int Max3DTextureSize, int MaxCubeMapTextureSize, int MaxRectangleTextureSize,
        int MaxRenderBufferSize, int MaxVertexAttributes, int MaxArrayTextureLayers, int MaxFrameBufferColorAttachments, int MaxDrawBuffers, int MaxClipDistances,
        int MaxTransformFeedbackBuffers, int MaxTransformFeedbackInterleavedComponents, int MaxTransformFeedbackSeparateComponents,
        int MaxTransformFeedbackSeparateAttributes, int MaxShaderStorageBufferBindings, int MaxAtomicCounterBufferBindings, int MaxFragmentUniformComponents,
        int MaxUniformLocations, int MaxVaryingComponents
        );
}
