using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WallApp.Scripting
{
    public abstract class SettingsController
    {
        public LayerSettings Settings { get; set; }

        public abstract Control GetSettingsControl();
        public abstract string ApplyClicked();
        public abstract void CancelClicked();
    }
}
