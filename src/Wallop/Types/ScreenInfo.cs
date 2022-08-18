using Monitor = Silk.NET.Windowing.Monitor;
using System.Numerics;

namespace Wallop.Types
{
    public class ScreenInfo
    {
        public static int GetScreenCount()
        {
            return Monitor.GetMonitors(null).Count();
        }

        public static ScreenInfo[] GetScreens()
        {
            var screens = new ScreenInfo[GetScreenCount() + 1];

            screens[0] = GetVirtualScreen();
            var physical = GetPhysicalScreens();
            Array.Copy(screens, 1, physical, 0, physical.Length);

            return screens;
        }

        public static ScreenInfo[] GetPhysicalScreens()
        {
            var monitors = Monitor.GetMonitors(null).ToArray();
            var screens = new ScreenInfo[monitors.Length];
            for (int i = 0; i < monitors.Length; i++)
            {
                var monitor = monitors[i];
                var refreshRate = monitor.VideoMode.RefreshRate ?? -1;

                Vector4 bounds = new Vector4(monitor.Bounds.Origin.X, monitor.Bounds.Origin.Y, monitor.Bounds.Size.X, monitor.Bounds.Size.Y);
                Vector2? resolution = null;

                if(monitor.VideoMode.Resolution != null)
                {
                    resolution = new Vector2(monitor.VideoMode.Resolution.Value.X, monitor.VideoMode.Resolution.Value.Y);
                }

                screens[i] = new ScreenInfo(monitors[i].Name, bounds, resolution, refreshRate);
            }
            return screens;
        }

        public static ScreenInfo GetVirtualScreen()
        {
            var bounds = new Vector4();
            int refreshRate = int.MaxValue;

            var screens = GetPhysicalScreens();
            var xStarts = new List<float>(screens.Length);
            var yStarts = new List<float>(screens.Length);

            for (int i = 0; i < screens.Length; i++)
            {
                ScreenInfo screen = screens[i];

                if(!xStarts.Any(xs => xs == screen.Bounds.X))
                {
                    if(xStarts.All(xs => xs >= screen.Bounds.X))
                    {
                        bounds.X = screen.Bounds.X;
                    }

                    xStarts.Add(screen.Bounds.X);
                    bounds.Z += screen.Bounds.Z;
                }
                if (!yStarts.Any(ys => ys == screen.Bounds.Y))
                {
                    if (yStarts.All(ys => ys >= screen.Bounds.Y))
                    {
                        bounds.Y = screen.Bounds.Y;
                    }

                    yStarts.Add(screen.Bounds.Y);
                    bounds.W += screen.Bounds.W;
                }

                if(screen.RefreshRate < refreshRate)
                {
                    refreshRate = screen.RefreshRate;
                }
            }
            return new ScreenInfo("extended", bounds, null, refreshRate);
        }

        public string Name { get; private set; }
        public Vector4 Bounds { get; private set; }
        public Vector2 NativeResolution { get; private set; }
        public int RefreshRate { get; private set; }


        internal ScreenInfo(string name, Vector4 bounds, Vector2? resolution, int refreshRate)
        {
            Name = name;
            Bounds = bounds;
            NativeResolution = resolution ?? Vector2.Zero;
            RefreshRate = refreshRate;
        }
    }
}
