namespace Wallop.Bridge.Data
{
    public class LayerCreationResponsePayload : IPayload
    {
        public int LayerId { get; private set; }

        public LayerCreationResponsePayload(int layerId)
        {
            LayerId = layerId;
        }
    }
}
