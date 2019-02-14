using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallApp.UI.ViewModels;

namespace WallApp.UI.Models
{
    public abstract class SettingsModel
    {
        public abstract string GetFrameRate();
        public abstract void SetFrameRate(string frameRate);
        public abstract string GetBackBufferScale();
        public abstract void SetBackBufferScale(string scale);
        public abstract IEnumerable<ModuleItemViewModel> GetModuleItems();
        public abstract IEnumerable<LayerItemViewModel> GetLayerItems();
    }
}
