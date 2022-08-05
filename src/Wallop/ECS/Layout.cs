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
        public Vector2 PresentationSize
        {
            get => _presentationSize;
            set
            {
                //if(value.X > Screen.Bounds.Size.X ||
                //    value.Y > Screen.Bounds.Size.Y)
                //{
                //    throw new ArgumentException("Actual size must be contained within the screen's bounds.", nameof(value));
                //}
                _presentationSize = value;
            }
        }

        private Vector2 _presentationSize;

        public Layout(string name)
        {
            Name = name;
            Screen = ScreenInfo.GetVirtualScreen();
            EntityRoot = new Manager();
            _presentationSize = Vector2.Zero;
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
