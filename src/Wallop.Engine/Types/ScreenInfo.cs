using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Windowing;
using Monitor = Silk.NET.Windowing.Monitor;

namespace Wallop.Engine.Types
{
    public class ScreenInfo
    {
        public static int GetScreenCount()
        {
            return Monitor.GetMonitors(null).Count();
        }

        public static ScreenInfo[] GetScreens()
        {
            var monitors = Monitor.GetMonitors(null).ToArray();
            var screens = new ScreenInfo[monitors.Length];
            for (int i = 0; i < monitors.Length; i++)
            {
                var bounds = monitors[i].Bounds;
                var refreshRate = monitors[i].VideoMode.RefreshRate ?? -1;
                screens[i] = new ScreenInfo(monitors[i].Name, bounds, monitors[i].VideoMode.Resolution, refreshRate);
            }
            return screens;
        }

        public static ScreenInfo GetVirtualScreen()
        {
            var bounds = new Rectangle<int>();
            int refreshRate = int.MaxValue;

            var screens = GetScreens();
            var xStarts = new List<int>(screens.Length);
            var yStarts = new List<int>(screens.Length);

            for (int i = 0; i < screens.Length; i++)
            {
                ScreenInfo screen = screens[i];

                if(!xStarts.Any(xs => xs == screen.Bounds.Origin.X))
                {
                    if(xStarts.All(xs => xs >= screen.Bounds.Origin.X))
                    {
                        bounds.Origin.X = screen.Bounds.Origin.X;
                    }

                    xStarts.Add(screen.Bounds.Origin.X);
                    bounds.Size.X += screen.Bounds.Size.X;
                }
                if (!yStarts.Any(ys => ys == screen.Bounds.Origin.Y))
                {
                    if (yStarts.All(ys => ys >= screen.Bounds.Origin.Y))
                    {
                        bounds.Origin.Y = screen.Bounds.Origin.Y;
                    }

                    yStarts.Add(screen.Bounds.Origin.Y);
                    bounds.Size.Y += screen.Bounds.Size.Y;
                }

                if(screen.RefreshRate < refreshRate)
                {
                    refreshRate = screen.RefreshRate;
                }
            }
            return new ScreenInfo("[VIRT]", bounds, null, refreshRate);
        }

        public string Name { get; private set; }
        public Rectangle<int> Bounds { get; private set; }
        public Vector2D<int>? NativeResolution { get; private set; }
        public int RefreshRate { get; private set; }


        internal ScreenInfo(string name, Rectangle<int> bounds, Vector2D<int>? resolution, int refreshRate)
        {
            Name = name;
            Bounds = bounds;
            NativeResolution = resolution;
            RefreshRate = refreshRate;
        }
    }
}
