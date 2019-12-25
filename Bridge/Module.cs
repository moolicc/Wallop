namespace WallApp.Bridge
{
    public class Module
    {
        public Manifest Manifest { get; private set; }

        public Module()
        {
        }

        public Module(Manifest manifest)
        {
            Init(manifest);
        }

        internal void Init(Manifest manifest)
        {
            Manifest = manifest;
            Initialize();
        }

        protected virtual void Initialize()
        { }
    }
}
