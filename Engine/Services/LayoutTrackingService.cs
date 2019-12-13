using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.Engine.Services
{
    [Service]
    class LayoutTrackingService : InitializableService
    {
        public Layout Layout { get; private set; }

        public void Init(Layout layout)
        {
            Layout = layout;
        }

        protected override void Initialize()
        {
            var proxyservice = ServiceProvider.GetService<BridgeMessageProxy>();

            proxyservice.LayerCreated += OnCreateLayer;

            base.Initialize();
        }

        private void OnCreateLayer(string module, BridgeService bridgeService)
        {
            Layout
        }
    }
}
