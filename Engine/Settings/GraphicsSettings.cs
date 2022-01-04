using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Settings
{
    internal class GraphicsSettings : Cog.Settings
    {
        public Veldrid.GraphicsBackend Backend { get; set; }
        public int WindowWidth { get; set; } = 540;
        public int WindowHeight { get; set; } = 960;
    }
}
