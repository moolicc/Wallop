using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.App.Services
{
    class BridgeService : IService
    {
        public int InitPriority => 0;
        public Bridge.Master Engine { get; private set; }
        public BridgeMessageScheduler Scheduler { get; private set; }

        public void Initialize()
        {
            Engine = new Bridge.Master(App.BaseDir + "MockEngine.exe");
            Scheduler = new BridgeMessageScheduler(new Bridge.InputReader<Bridge.Data.IPayload>(Engine));
        }

        public void SetEditMode(bool editModeEnabled)
        {
            Engine.Write(new Bridge.Data.EditModePayload(editModeEnabled));
        }

        public void AddLayer(string module)
        {
            Engine.Write(new Bridge.Data.LayerCreationPayload(module));
        }

        public void PositionLayer(int layerId, Layout.LayerDimensions dimensions)
        {
        }
    }
}
