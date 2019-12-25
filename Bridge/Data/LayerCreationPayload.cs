namespace WallApp.Bridge.Data
{
    public class LayerCreationPayload : IPayload
    {
        public string Module { get; set; }

        public LayerCreationPayload(string module)
        {
            Module = module;
        }

        public override string ToString()
        {
            return $"{nameof(LayerCreationPayload)} : {nameof(Module)} = \"{Module}\"";
        }
    }
}
