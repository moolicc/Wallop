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
    }
}
