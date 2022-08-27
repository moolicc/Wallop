using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS;
using Wallop.Types;

namespace Wallop.ECS
{
    public class Layout : ILayout
    {

        public string Name { get; set; }
        public Manager EntityRoot { get; set; }
        public ScreenInfo Screen
        {
            get => _screen;
            set
            {
                _screen = value;
                if (RenderSize.X > _screen.NativeResolution.X || RenderSize.Y > _screen.NativeResolution.Y
                    || RenderSize.X <= 1 || RenderSize.Y <= 1)
                {
                    RenderSize = _screen.NativeResolution;// / 2;
                }
            }
        }
        public bool IsActive { get; private set; }

        public Vector2 RenderSize { get; set; }

        // TODO: Implement this, including exposing related functionality to the gui and scripts.
        public Vector4 PresentationBounds { get; set; }

        private ScreenInfo _screen;

        public Layout(string name)
        {
            Name = name;
            _screen = ScreenInfo.GetVirtualScreen();
            EntityRoot = new Manager();
            RenderSize = new Vector2(_screen.NativeResolution.X, _screen.NativeResolution.Y);
            PresentationBounds = new Vector4(0, 0, 0, 0);
        }

        public void Activate()
        {
            IsActive = true;
        }
        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
