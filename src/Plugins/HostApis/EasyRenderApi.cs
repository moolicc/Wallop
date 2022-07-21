using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TrippyGL;
using Wallop.DSLExtension.Scripting;
using Wallop.DSLExtension.Types.Plugin;
using Wallop.Scripting;

namespace HostApis
{
    public class EasyRenderApi : IHostApi
    {
        public string Name => "EzRender";

        public void Use(IScriptContext scriptContext)
        {
            var device = TrippyGLApi.GetDevice(scriptContext);
            var shader = SimpleShaderProgram.Create<VertexColorTexture>(device, 0, 0, true);
            var size = scriptContext.GetRenderSize();
            var batcher = new TextureBatcher(device);

            shader.Projection = Matrix4x4.CreateOrthographicOffCenter(0.0f, size.X, size.Y, 0.0f, 0.0f, 1.0f);
            batcher.SetShaderProgram(shader);

            var functions = new EasyFunctions(scriptContext, device, shader, batcher);
        }

        public void BeforeDraw(IScriptContext scriptContext, double delta)
        {
            var batcher = scriptContext.GetValue<TextureBatcher>(EasyFunctions.VAR_BATCHER);
            batcher?.Begin();
        }

        public void BeforeUpdate(IScriptContext scriptContext, double delta)
        {
        }

        public void AfterDraw(IScriptContext scriptContext)
        {
            var batcher = scriptContext.GetValue<TextureBatcher>(EasyFunctions.VAR_BATCHER);
            batcher?.End();
        }

        public void AfterUpdate(IScriptContext scriptContext)
        {
        }
    }
}
