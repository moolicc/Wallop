using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TrippyGL;
using Wallop.Scripting;
using Wallop.Shared.Scripting;
using Wallop.Shared.Types.Plugin;

namespace HostApis
{
    public class TrippyGLApi : IHostApi
    {
        public string Name => "TrippyGL";

        private static GraphicsDevice _device;

        internal static GraphicsDevice GetDevice(IScriptContext context)
        {
            if (_device == null)
            {
                var gl = context.GetGLInstance();

                var maj = gl.GetInteger(GLEnum.MajorVersion);
                var min = gl.GetInteger(GLEnum.MinorVersion);
                _device = new GraphicsDevice(gl);
                _device.ClearColor = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
                _device.DepthState = DepthState.None;
                _device.BlendState = BlendState.NonPremultiplied;

                _device.CullFaceMode = CullingMode.CullBack;
                _device.FaceCullingEnabled = false;
            }
            return _device;
        }


        public void Use(IScriptContext scriptContext)
        {
            var device = GetDevice(scriptContext);
            scriptContext.AddReference(device.GetType().Assembly);
            scriptContext.AddReference(typeof(TrippyGL.ImageSharp.Texture2DExtensions).Assembly);
            scriptContext.AddImport("TrippyGL");
            scriptContext.AddImport("TrippyGL.ImageSharp");
            scriptContext.SetValue("GraphicsDevice", _device);
        }


        public void AfterDraw(IScriptContext scriptContext)
        {
        }

        public void AfterUpdate(IScriptContext scriptContext)
        {
        }

        public void BeforeDraw(IScriptContext scriptContext, double delta)
        {
        }

        public void BeforeUpdate(IScriptContext scriptContext, double delta)
        {
        }

    }
}
