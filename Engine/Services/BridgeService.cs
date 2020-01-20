using System.Collections.Generic;
using WallApp.Bridge;

namespace WallApp.Engine.Services
{
    [Service()]
    public class BridgeService : InitializableService
    {
        public MessageScheduler Scheduler { get; private set; }
        private Slave _engine;

        protected override void Initialize()
        {
            _engine = new Slave();
            Scheduler = new MessageScheduler(new InputReader<Bridge.Data.IPayload>(_engine));
            base.Initialize();
        }

        //TODO: Add WriteX functions to talk to the master.
        public void WriteEditModeResponse(IEnumerable<string> layerNames, IEnumerable<(float x, float y, float z, float w)> layerDimensions)
        {
            var response = new Bridge.Data.EditModeResponse();
            response.Layers.AddRange(layerNames);
            response.LayerPositions.AddRange(layerDimensions);
            _engine.Write(response);
        }

        public void WriteCreateLayerResponse(int layerId)
        {
            _engine.Write(new Bridge.Data.LayerCreationResponsePayload(layerId));
        }
    }
}
