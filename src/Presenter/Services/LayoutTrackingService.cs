namespace Wallop.Presenter.Services
{
    public delegate void LayerAdded(LayerSettings layerSettings);
    public delegate void LayerResized(LayerSettings layerSettings);
    public delegate void LayerRemoved(int layerId);

    [Service]
    class LayoutTrackingService : InitializableService
    {
        public event LayerAdded LayerAdded;
        public event LayerRemoved LayerRemoved;
        public event LayerResized LayerResized;

        [ServiceReference]
        public Layout Layout { get; private set; }

        public void Init(Layout layout)
        {
            Layout = layout;
        }

        protected override void Initialize()
        {
            var proxyservice = ServiceProvider.GetService<BridgeMessageProxy>();

            proxyservice.LayerCreated += OnCreateLayer;
            proxyservice.LayerDimensionsChanged += OnLayerDimensionsChange;
            proxyservice.LayerDeleted += OnLayerDeleted;
            proxyservice.LayerSettingsChanged += OnLayerSettingsChanged;

            base.Initialize();
        }

        private void OnCreateLayer(string module, BridgeService bridgeService)
        {
            var settings = Layout.AddLayer(module);
            settings.DimensionsChanged += LayerDimensionsChanged;
            bridgeService.WriteCreateLayerResponse(settings.LayerId);
            LayerAdded?.Invoke(settings);
        }

        private void OnLayerDimensionsChange(int layerId, (float x, float y, float z, float w) dimensions, bool useAbsolutes, bool useMargins, BridgeService bridgeService)
        {
            Layout.GetLayer(layerId).Dimensions.Set(dimensions.x, dimensions.y, dimensions.z, dimensions.w, useAbsolutes, useMargins);
        }

        private void LayerDimensionsChanged(object sender)
        {
            LayerResized?.Invoke((LayerSettings)sender);
        }

        private void OnLayerDeleted(int layerId)
        {
            Layout.RemoveLayer(layerId);
            LayerRemoved?.Invoke(layerId);
        }
        private void OnLayerSettingsChanged(int layerId, bool enabled, float rotation, string effect, Microsoft.Xna.Framework.Color tint, float opacity)
        {
            var layer = Layout.GetLayer(layerId);
            layer.Enabled = enabled;
            layer.Rotation = rotation;
            layer.Effect = effect;
            layer.TintColor = tint;
            layer.Opacity = opacity;
        }
    }
}
