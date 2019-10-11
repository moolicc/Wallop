using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallApp.Bridge.Data;

namespace WallApp.App.Layout.ScriptTypes
{
    public sealed class LayoutScriptContext
    {
        Services.BridgeService _bridgeService;

        public LayoutScriptContext()
        {
            _bridgeService = Services.ServiceLocator.Locate<Services.BridgeService>();
        }

        public int CreateLayer(string module)
        {
            _bridgeService.AddLayer(module);

            var payload = _bridgeService.Scheduler.ConsumeNext<LayerCreationResponsePayload>();

            return payload.LayerId;
        }

        public IEnumerable<object> GetLayers()
        {
            return null;
        }


        // Misc. Functions.

        public void SetLayerVisibility(bool visible)
        {

        }


        // Dimension Functions

        public void SetReferenceMonitor(string adapter)
        {

        }

        public void SetDimensions(float x, float y, float z, float w)
        {

        }

        public void SetAbsoluteDimensions(bool useAbsolutePixels)
        {

        }

        public void SetMarginDimensions(bool useMargins)
        {

        }


        // Layer VarTable Functions

        public void SetField(string field, object value)
        {

        }

        public object GetField(string field)
        {
            return null;
        }
    }
}
