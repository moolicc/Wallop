using WallApp.Bridge.Data;

namespace WallApp.Engine.Services
{
    namespace BridgeMessages
    {
        public delegate void EditModeEventHandler(bool enabled, BridgeService bridgeService);
        public delegate void LayerCreationEventHandler(string module, BridgeService bridgeService);
        public delegate void LayerDimensionsChangedEventHandler(int layerId, (float x, float y, float z, float w) dimensions, bool useAbsolutes, bool useMargins, BridgeService bridgeService);
    }

    [Service]
    class BridgeMessageProxy : InitializableService
    {
        public event BridgeMessages.EditModeEventHandler EditModeChanged;
        public event BridgeMessages.LayerCreationEventHandler LayerCreated;
        public event BridgeMessages.LayerDimensionsChangedEventHandler LayerDimensionsChanged;

        [ServiceReference]
        private BridgeService _bridgeService;

        public BridgeMessageProxy()
        {
        }

        protected override void Initialize()
        {
            _bridgeService.Scheduler.RegisterMessage<EditModePayload>(OnEditModeChanged, -1, null);
            _bridgeService.Scheduler.RegisterMessage<LayerCreationPayload>(OnLayerCreated, -1, null);
            _bridgeService.Scheduler.RegisterMessage<PositionPayload>(OnLayerDimensionsChanged, -1, null);
            base.Initialize();
        }

        private void OnEditModeChanged(IPayload payload)
        {
            EditModeChanged?.Invoke((payload as EditModePayload).Enabled, _bridgeService);
        }

        private void OnLayerCreated(IPayload payload)
        {
            LayerCreated?.Invoke((payload as LayerCreationPayload).Module, _bridgeService);
        }

        private void OnLayerDimensionsChanged(IPayload payload)
        {
            var dimensionsPayload = payload as PositionPayload;
            var (x, y, z, w) = dimensionsPayload;
            LayerDimensionsChanged?.Invoke(dimensionsPayload.LayerId, (x, y, z, w), dimensionsPayload.UseAbsolutes, dimensionsPayload.UseMargins, _bridgeService);
        }
    }
}
