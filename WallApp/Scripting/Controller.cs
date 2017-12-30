using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WallApp.Scripting
{
    public abstract class Controller
    {
        public LayerSettings LayerSettings { get; set; }
        public Module Module { get; internal set; }
        public GraphicsDevice GraphicsDevice { get; internal set; }
        public RenderTarget2D RenderTarget { get; internal set; }
        public Rectangle ScaledLayerBounds { get; internal set; }
        public Action<Control> PlaceControl { get; internal set; }
        
        public abstract void Setup();
        public abstract void EnabledChanged();
        public abstract void Dispose();

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);
    }
}
