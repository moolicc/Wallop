using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Scripting;

namespace StandardComponents
{
    public class BoundsComponent : BindableType
    {
        public float X
        {
            get => GetValue(nameof(X), ref _x);
            set => SetValue(nameof(X), value, ref _x);
        }

        public float Y
        {
            get => GetValue(nameof(Y), ref _y);
            set => SetValue(nameof(Y), value, ref _y);
        }

        public float Width
        {
            get => GetValue(nameof(Width), ref _width);
            set => SetValue(nameof(Width), value, ref _width);
        }

        public float Height
        {
            get => GetValue(nameof(Height), ref _height);
            set => SetValue(nameof(Height), value, ref _height);
        }

        private float _x;
        private float _y;
        private float _width;
        private float _height;
    }
}
