using Silk.NET.OpenAL;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TrippyGL;
using TrippyGL.ImageSharp;
using Wallop.Scripting;
using Wallop.Shared.Scripting;
using Wallop.Shared.Types.Plugin;

namespace HostApis
{
    public class EasyRenderApi : IHostApi
    {
        public string Name => "EzRender";

        private EasyFunctions _functions;

        private Vector2 _lastSize;

        public void Use(IScriptContext scriptContext)
        {
            var device = TrippyGLApi.GetDevice(scriptContext);
            var shader = SimpleShaderProgram.Create<VertexColorTexture>(device, 0, 0, true);
            var batcher = new TextureBatcher(device);

            _functions = new EasyFunctions(scriptContext, device, shader, batcher);
            _lastSize = scriptContext.GetRenderSize();
            OnResized();

        }

        public void BeforeDraw(IScriptContext scriptContext, double delta)
        {
            var curSize = scriptContext.GetRenderSize();
            if(curSize != _lastSize)
            {
                _lastSize = curSize;
                OnResized();
            }
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

        private void OnResized()
        {
            _functions.Shader.Projection = Matrix4x4.CreateOrthographicOffCenter(0.0f, _lastSize.X, _lastSize.Y, 0.0f, 0.0f, 1.0f);
            _functions.Batcher.SetShaderProgram(_functions.Shader);
        }
    }
}
