using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.App.Layout
{
    class Layer
    {
        public LayerDimensions Dimensions { get; private set; }
        public Modules.Module Module { get; private set; }
        public Dictionary<string, object> VarTable { get; private set; }
        public int Id { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Layer(Modules.Module module, int layerId)
        {
            Dimensions = new LayerDimensions();
            Module = module;
            VarTable = new Dictionary<string, object>();
            Id = layerId;
            Name = $"Layer {layerId}";
            Description = "";
        }
    }
}
