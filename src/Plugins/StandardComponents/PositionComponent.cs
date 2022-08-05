using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Scripting;

namespace StandardComponents
{
    public class PositionComponent : BindableType
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

        private float _x;
        private float _y;
    }
}
