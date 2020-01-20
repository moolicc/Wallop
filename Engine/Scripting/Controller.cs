using Microsoft.Xna.Framework;
using WallApp.Bridge;

namespace WallApp.Engine.Scripting
{
    public abstract class Controller
    {
        public LayerSettings Settings { get; set; }
        public Module Module { get; internal set; }
        public Rendering Rendering { get; internal set; }

        public int LayerId { get => Settings.LayerId; }

        public abstract void Setup();
        public abstract void Dispose();

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);
    }
}
