using System;
using System.Collections.Generic;
using System.Text;
using WallApp.UI.Models;

namespace WallApp.UI.Interop
{
    class ModelProvider : UI.ModelProvider
    {
        public Game GameInsance { get; private set; }

        public ModelProvider(Game gameInstance)
        {
            GameInsance = gameInstance;
        }

        public override Models.LayerSettingsModel GetLayerSettingsModel()
        {
            return new LayerSettingsModel();
        }

        public override Models.SettingsModel GetSettingsModel()
        {
            return new SettingsModel();
        }
    }
}
