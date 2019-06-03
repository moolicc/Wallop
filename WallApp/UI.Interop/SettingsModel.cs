using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallApp.UI.ViewModels;

namespace WallApp.UI.Interop
{
    class SettingsModel : Models.SettingsModel
    {
        public Layout Layout { get; private set; }

        public override double BackBufferScale
        {
            get => Settings.Instance.BackBufferWidthFactor;
            set
            {
                Settings.Instance.BackBufferWidthFactor = value;
                Settings.Instance.BackBufferHeightFactor = value;
            }
        }
        public override int FrameRate
        {
            get => Settings.Instance.FrameRate;
            set
            {
                Settings.Instance.FrameRate = value;
                Settings.Instance.FrameRate = value;
            }
        }

        public SettingsModel(Layout layout)
        {
            Layout = layout;
        }


        public override int GetLayerCount()
        {
            return Layout.Layers.Count;
        }

        public override int GetModuleCount()
        {
            return Scripting.Resolver.Cache.Values.Count;
        }

        public override void Apply()
        {
            (UI.ModelProvider.Instance as ModelProvider).GameInsance.ResetGraphicsSettings();
        }
    }
}
