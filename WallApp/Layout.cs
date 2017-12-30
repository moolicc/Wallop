using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp
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
