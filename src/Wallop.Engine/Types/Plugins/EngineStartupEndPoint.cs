using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Types.Plugins
{
    public class EngineStartupEndPoint
    {
        public Settings.GraphicsSettings GraphicsSettings { get; set; }
        public int WindowWidth => GraphicsSettings.WindowWidth;
    }
}
