using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallApp.Bridge.Data;

namespace WallApp.Engine.Services
{
    public delegate void EditModeHandler(bool enabled, BridgeService bridgeService);

    [Service]
    class BridgeMessageProxy : InitializableService
    {
        public event EditModeHandler EditModeChanged;

        [ServiceReference]
        private BridgeService _bridgeService;

        // TODO: Add events for the messages we might have incoming. The events should contain the bridge service as a parameter
        // so that whatever is handling it can spit something back to the master. We also need to hook into _bridgeservice.scheduler
        // to get the messages from the master to push through as the events contained herein.

        public BridgeMessageProxy()
        {
        }

        protected override void Initialize()
        {
            _bridgeService.Scheduler.RegisterMessage<EditModePayload>(OnEditModeChanged, -1, null);
            base.Initialize();
        }

        private void OnEditModeChanged(IPayload payload)
        {
            EditModeChanged?.Invoke((payload as EditModePayload).Enabled, _bridgeService);
        }
    }
}
