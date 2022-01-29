using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.Types;

namespace Wallop.Engine.SceneManagement
{
    public class Layout
    {
        public ECS.Manager EcsRoot { get; set; }
        public ScreenInfo Screen { get; set; }

        public Vector2 RenderSize { get; set; }
        public Vector2 ActualSize
        {
            get => _actualSize;
            set
            {
                //if(value.X > Screen.Bounds.Size.X ||
                //    value.Y > Screen.Bounds.Size.Y)
                //{
                //    throw new ArgumentException("Actual size must be contained within the screen's bounds.", nameof(value));
                //}
                _actualSize = value;
            }
        }

        private Vector2 _actualSize;

        public Layout()
        {
            EcsRoot = new ECS.Manager();
        }
    }
}
