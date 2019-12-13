using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallApp.Bridge.Data;

namespace WallApp.Engine.Services
{
    public delegate void EditModeHandler(bool enabled, BridgeService bridgeService);
    public delegate void LayerCreationHandler(string module, BridgeService bridgeService);

    [Service]
    class BridgeMessageProxy : InitializableService
    {
        public event EditModeHandler EditModeChanged;
        public event LayerCreationHandler LayerCreated;

        [ServiceReference]
        private BridgeService _bridgeService;

        public BridgeMessageProxy()
        {
        }

        protected override void Initialize()
        {
            _bridgeService.Scheduler.RegisterMessage<EditModePayload>(OnEditModeChanged, -1, null);
            _bridgeService.Scheduler.RegisterMessage<LayerCreationPayload>(OnLayerCreated, -1, null);
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
    }
}
