using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Sdl2;

namespace Wallop.Engine.Types.Plugins.EndPoints
{
    public class OverlayerEndPoint : EndPointBase
    {
        public Sdl2Window SdlWindow { get; private set; }
        public IntPtr WindowHandle => SdlWindow.Handle;

        internal OverlayerEndPoint(Sdl2Window window)
        {
            SdlWindow = window;
        }
    }
}
