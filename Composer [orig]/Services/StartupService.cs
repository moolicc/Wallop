using System;

namespace Wallop.Composer.Services
{
    class StartupService
    {
        public event EventHandler StartupComplete;

        public void InitApp()
        {
            ServiceLocator.LocateAllServices();

            //TODO: Check for updates
            //TODO: Load layouts

            StartupComplete?.Invoke(this, new EventArgs());
        }
    }
}
