using System.Collections.Generic;

namespace WallApp.Engine
{
    class Layout
    {
        //TODO: Recycle IDs


        public IEnumerable<LayerSettings> Layers => _layers.Values;

        private Dictionary<int, LayerSettings> _layers;
        private int _nextId;

        public Layout()
        {
            _layers = new Dictionary<int, LayerSettings>();
            _nextId = 0;
        }

        public LayerSettings AddLayer(string module)
        {
            var settings = new LayerSettings(_nextId, module);
            _layers.Add(_nextId, settings);
            _nextId++;
            return settings;
        }

        public void RemoveLayer(int layerId)
        {
            _layers.Remove(layerId);
        }

        public LayerSettings GetLayer(int layerId)
        {
            return _layers[layerId];
        }
    }
}
