﻿using EnginePlugins.Overlay.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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

        public void OverlayWindowEndPoint_Windows(Silk.NET.Windowing.IWindow window)
        {

            var bounds = Wallop.Engine.Types.ScreenInfo.GetVirtualScreen().Bounds;
            Console.WriteLine("Setting SDL window bounds: {{ {0}, {1}, {2}, {3} }}", 0, 0, bounds.Size.X, bounds.Size.Y);
            window.Position = new Silk.NET.Maths.Vector2D<int>(0, 0);
            window.Size = new Silk.NET.Maths.Vector2D<int>(bounds.Size.X, bounds.Size.Y);
            window.WindowBorder = Silk.NET.Windowing.WindowBorder.Hidden;

            Console.WriteLine("Setting parent for handler: 0x{0:X}", window.Handle);
            WindowHandler.SetParet(window.Handle);
        }

        public void OverlayWindowEndPoint_Linux(IntPtr hWnd)
        {
            Console.WriteLine("Setting parent for handler: 0x{0:X}", hWnd);
        }
    }
}
