using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.UI
{
    public abstract class ModelProvider
    {
        public static ModelProvider Instance { get; private set; }

        protected ModelProvider()
        {
            Instance = this;
        }

        public abstract Models.LayerSettingsModel GetLayerSettingsModel();
        public abstract Models.SettingsModel GetSettingsModel();
    }
}
