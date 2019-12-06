using System.Collections.Generic;

namespace WallApp.Engine
{
    class Layout
    {
        public List<LayerSettings> Layers { get; private set; }

        public Layout()
        {
            Layers = new List<LayerSettings>();
        }
    }
}
