using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallApp.Bridge.Data;

namespace WallApp.Engine.Services
{
    namespace BridgeMessages
    {
        public delegate void EditModeEventHandler(bool enabled, BridgeService bridgeService);
        public delegate void LayerCreationEventHandler(string module, BridgeService bridgeService);
    }

    [Service]
    class BridgeMessageProxy : InitializableService
    {
        public event BridgeMessages.EditModeEventHandler EditModeChanged;
        public event BridgeMessages.LayerCreationEventHandler LayerCreated;

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
