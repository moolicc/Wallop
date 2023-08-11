using EnginePlugins.Overlay.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wallop.Types.Plugins.EndPoints;

namespace EnginePlugins
{
    public class OverlayerPlugin
    {
        [PluginPantry.EntryPoint(new string[] { "name", "Overlayer Backend", "version", "1.0.0.0" })]
        public static void PluginEntryPoint(PluginPantry.PluginContext context, Guid guid)
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                context.RegisterAction<OverlayerEndPoint, OverlayerPlugin>(guid, nameof(OverlayWindowEndPoint_Windows));
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                context.RegisterAction<OverlayerEndPoint, OverlayerPlugin>(guid, nameof(OverlayWindowEndPoint_Linux));
            }
        }

        public static void OverlayWindowEndPoint_Windows(Silk.NET.Windowing.IWindow window)
        {

            var bounds = Wallop.Types.ScreenInfo.GetVirtualScreen().Bounds;
            Console.WriteLine("Setting SDL window bounds: {{ {0}, {1}, {2}, {3} }}", (int)bounds.X, (int)bounds.Y, (int)bounds.Z, (int)bounds.W);
            window.Position = new Silk.NET.Maths.Vector2D<int>(0, 0);
            window.Size = new Silk.NET.Maths.Vector2D<int>((int)bounds.Z, (int)bounds.W);
            window.WindowBorder = Silk.NET.Windowing.WindowBorder.Hidden;

            Console.WriteLine("Setting parent for handler: 0x{0:X}", window.Handle);

            var nativeInfo = window.Native?.Win32;

            if(nativeInfo != null)
            {
                WindowHandler.SetParet(nativeInfo.Value.Hwnd);
            }
            else
            {
                throw new PlatformNotSupportedException("Failed to find native window handle.");
            }
        }

        public void OverlayWindowEndPoint_Linux(IntPtr hWnd)
        {
            Console.WriteLine("Setting parent for handler: 0x{0:X}", hWnd);
        }
    }
}
