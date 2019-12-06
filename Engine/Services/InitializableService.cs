namespace WallApp.Engine.Services
{
    abstract class InitializableService
    {
        public bool Initialized { get; private set; }

        protected virtual void Initialize()
        {
            Initialized = true;
        }

        protected virtual void CheckInitialized()
        {
            if (!Initialized)
            {
                throw new ServiceNotInitializedException(GetType().Name);
            }
        }
    }
}
