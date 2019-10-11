using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.Scripting
{
    public class Rendering
    {
        public GraphicsDevice GraphicsDevice { get; internal set; }
        public RenderTarget2D RenderTarget { get; internal set; }
        public int ActualX { get; internal set; }
        public int ActualY { get; internal set; }
        public int ActualWidth { get; internal set; }
        public int ActualHeight { get; internal set; }

        public Rendering(GraphicsDevice device, RenderTarget2D renderTarget)
        {
            GraphicsDevice = device;
            RenderTarget = renderTarget;
        }
    }
}
