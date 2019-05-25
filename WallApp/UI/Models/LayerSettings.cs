using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WallApp.UI.Models
{
    public class LayerSettings
    {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual int ScreenIndex { get; set; }
        public virtual bool AbsToggle { get; set; }
        public virtual bool MarginsToggle { get; set; }
        public virtual (int X, int Y, int Z, int W) CurrentPosition { get; set; }
        public virtual (byte A, byte R, byte G, byte B) TintColor { get; set; }
        public virtual int EffectIndex { get; set; }
        public virtual bool Enabled { get; set; }
        public virtual int ModuleIndex { get; set; }
        public virtual string ModuleName { get; set; }
        public virtual int ID { get; set; }

    }
}
