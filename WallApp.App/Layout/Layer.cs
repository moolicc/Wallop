using System.Collections.Generic;

namespace WallApp.App.Layout
{
    class Layer
    {
        public LayerDimensions Dimensions { get; private set; }
        public Bridge.Manifest Module { get; private set; }
        public Dictionary<string, object> VarTable { get; private set; }
        public int Id { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Layer(Bridge.Manifest module, int layerId)
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
