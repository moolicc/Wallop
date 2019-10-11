using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.App.Services
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
