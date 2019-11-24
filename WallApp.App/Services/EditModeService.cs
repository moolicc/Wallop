using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WallApp.App.Services
{
    class EditModeService : IService
    {
        public int InitPriority => int.MaxValue;
        public bool LayoutUpdated { get; private set; }

        private Layout.LayoutInfo _trackingLayout;

        public void Initialize()
        {
            LayoutUpdated = false;
        }

        public void StartEdit(Layout.LayoutInfo layoutInfo)
        {
            if(_trackingLayout != null)
            {
                throw new InvalidOperationException("A layout is already being tracked. Make sure LayoutUpdated has been set to 'true' before attempting to modify a layer.");
            }
            LayoutUpdated = false;
            _trackingLayout = layoutInfo;
            ServiceLocator.Locate<BridgeService>().WriteSetEditMode(true);
        }

        public void StopEdit()
        {
            var bridge = ServiceLocator.Locate<BridgeService>();
            bridge.Scheduler.TakeNext<Bridge.Data.EditModeResponse>(new PayloadHandler(payload =>
            {
                //TODO: _trackingLayout needs to change based on whatever is in payload.

                LayoutUpdated = true;
                _trackingLayout = null;
            }), 3000, () => throw new InvalidOperationException("A response from the engine was expected but not received."));
            bridge.WriteSetEditMode(false);
        }

        public void AddLayer(string module)
        {
            ServiceLocator.Locate<BridgeService>().WriteAddLayer(module);
        }

        public IEnumerable<XElement> GenerateXmlScript()
        {
            return null;
        }
    }
}
