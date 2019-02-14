using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallApp.UI.ViewModels;

namespace WallApp.Windows
{
    class SettingsModel : WallApp.UI.Models.SettingsModel
    {


        public override string GetBackBufferScale()
        {
            return Settings.Instance.BackBufferWidthFactor.ToString();
        }

        public override string GetFrameRate()
        {
            return Settings.Instance.FrameRate.ToString();
        }

        public override IEnumerable<LayerItemViewModel> GetLayerItems()
        {
            return Layout.Layers.Select(l => new LayerItemViewModel()
            {
                Module = l.Module,
                ID = l.LayerId.ToString(),
                Name = l.Name,
                Description = l.Description,
                Enabled = l.Enabled
            });
        }

        public override IEnumerable<ModuleItemViewModel> GetModuleItems()
        {
            return Scripting.Resolver.Cache.Values.Select(m => new ModuleItemViewModel()
            {
                Description = m.Description,
                Directory = m.SourceFile,
                Name = m.Name,
                Version = m.Version.ToString()
            });
        }

        public override void SetBackBufferScale(string scale)
        {
            Settings.Instance.BackBufferWidthFactor = float.Parse(scale);
        }

        public override void SetFrameRate(string frameRate)
        {
            Settings.Instance.FrameRate = int.Parse(frameRate);
        }
    }
}
