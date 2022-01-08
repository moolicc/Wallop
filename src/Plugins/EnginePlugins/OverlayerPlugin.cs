using EnginePlugins.Overlay.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using Wallop.Engine.Types.Plugins.EndPoints;

namespace EnginePlugins
{
    public class OverlayerPlugin
    {
        [PluginPantry.Extending.PluginEntryPoint("Overlayer Backend", "1.0.0.0")]
        public void PluginEntryPoint(PluginPantry.Extending.PluginInformation pluginInfo)
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                pluginInfo.Exposed.RegisterEndPoint<OverlayerEndPoint>(nameof(OverlayWindowEndPoint_Windows), this, pluginInfo.PluginId);
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                pluginInfo.Exposed.RegisterEndPoint<OverlayerEndPoint>(nameof(OverlayWindowEndPoint_Linux), this, pluginInfo.PluginId);
            }
        }

        public void OverlayWindowEndPoint_Windows(Sdl2Window window)
        {

            var bounds = Wallop.Engine.Types.ScreenInfo.GetVirtualScreen().Bounds;
            Console.WriteLine("Setting SDL window bounds: {{ {0}, {1}, {2}, {3} }}", bounds.X, bounds.Y, bounds.Width, bounds.Height);
            window.X = 0;
            window.Y = 0;
            window.Width = bounds.Width;
            window.Height = bounds.Height;
            window.BorderVisible = false;

            Console.WriteLine("Setting parent for handler: 0x{0:X}", window.Handle);
            WindowHandler.SetParet(window.Handle);
        }

        public void OverlayWindowEndPoint_Linux(IntPtr hWnd)
        {
            Console.WriteLine("Setting parent for handler: 0x{0:X}", hWnd);
        }
    }
}
