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
        public ScreenInfo Screen { get; set; }
        public bool IsActive { get; private set; }

        public Vector2 RenderSize { get; set; }

        // TODO: Implement this, including exposing related functionality to the gui and scripts.
        public Vector4 PresentationBounds { get; set; }

        public Layout(string name)
        {
            Name = name;
            Screen = ScreenInfo.GetVirtualScreen();
            EntityRoot = new Manager();
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
