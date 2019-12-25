namespace WallApp.Bridge.Data
{
    public class EditModePayload : IPayload
    {
        public bool Enabled { get; set; }

        public EditModePayload(bool enabled)
        {
            Enabled = enabled;
        }

        public override string ToString()
        {
            return $"{nameof(EditModePayload)} : {nameof(Enabled)} = {Enabled}";
        }
    }
}
