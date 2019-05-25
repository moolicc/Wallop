namespace WallApp.UI.Models
{
    public abstract class SettingsModel
    {
        public virtual int FrameRate { get; set; }
        public virtual double BackBufferScale { get; set; }
        public abstract int GetModuleCount();
        public abstract int GetLayerCount();

        public abstract void Apply();
    }
}
