using System;
using System.Xml.Linq;
using Wallop.Bridge;

namespace Wallop.Composer.Services
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
            if (_trackingLayout != null)
            {
                throw new InvalidOperationException("A layout is already being tracked. Make sure LayoutUpdated is equal to 'true' before attempting to modify a layer.");
            }
            LayoutUpdated = false;
            _trackingLayout = layoutInfo;
            ServiceLocator.Locate<BridgeService>().WriteSetEditMode(true);
        }

        public void StopEdit()
        {
            var bridge = ServiceLocator.Locate<BridgeService>();
            bridge.Scheduler.RegisterMessage<Bridge.Data.EditModeResponse>(new PayloadHandler(payload =>
            {
                _trackingLayout.Script = GenerateXmlScript((Bridge.Data.EditModeResponse)payload);
                LayoutUpdated = true;
                _trackingLayout = null;
            }), 3000, () => throw new InvalidOperationException("A response from the engine was expected but not received."));
            bridge.WriteSetEditMode(false);
        }

        public void AddLayer(string module)
        {
            ServiceLocator.Locate<BridgeService>().WriteAddLayer(module);
        }

        private string GenerateXmlScript(Bridge.Data.EditModeResponse data)
        {
            var root = new XElement("layout");

            for (int i = 0; i < data.Layers.Count; i++)
            {
                var layerItem = new XElement("layer");
                layerItem.Add(new XElement("module", data.Layers[i]));

                layerItem.Add(new XElement("dimensions",
                    new XElement("x", data.LayerPositions[i].x),
                    new XElement("y", data.LayerPositions[i].y),
                    new XElement("z", data.LayerPositions[i].z),
                    new XElement("w", data.LayerPositions[i].w)));

                root.Add(layerItem);
            }


            return root.ToString();
        }
    }
}
