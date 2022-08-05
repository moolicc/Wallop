using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Scripting;

namespace StandardComponents
{
    public class SizeComponent : BindableType
    {
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

        private float _width;
        private float _height;
    }
}
