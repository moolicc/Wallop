namespace WallApp.App.Services
{
    /// <summary>
    /// The <see cref="LayoutScriptService"/> executes layout scripts and acts a bridge
    /// to the actual engine.
    /// </summary>
    class LayoutScriptService : IService
    {
        public int InitPriority => 100;

        public void Initialize()
        {
        }

        public int AddLayer(string module)
        {
            var bridgeService = ServiceLocator.Locate<BridgeService>();
            bridgeService.WriteAddLayer(module);
            var payload = bridgeService.Scheduler.ConsumeNext<Bridge.Data.LayerCreationResponsePayload>();
            return payload.LayerId;
        }

        public void SetReferenceMonitor(int layerId, string referenceMonitor)
        {
        }

        public void SetDimensions(int layerId, float posX, float posY, float posZ, float posW)
        {
        }

        public void SetAbsoluteDimensions(int layerId, bool absolutePos)
        {
        }

        public void SetMarginDimensions(int layerId, bool marginPos)
        {
        }
    }
}
