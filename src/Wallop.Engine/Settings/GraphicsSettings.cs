using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Settings
{
    public class GraphicsSettings : Cog.Settings
    {
       // public Veldrid.GraphicsBackend Backend { get; set; }
        public int WindowWidth { get; set; } = 540;
        public int WindowHeight { get; set; } = 960;
        public bool Overlay { get; set; }
        public WindowBorder WindowBorder { get; set; }




        public double RefreshRate { get; set; } = 60.0;
        public bool VSync { get; set; } = true;
    }
}
