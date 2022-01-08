using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace Wallop.Engine.Types
{
    public class ScreenInfo
    {
        public static int GetScreenCount()
        {
            return Veldrid.Sdl2.Sdl2Native.SDL_GetNumVideoDisplays();
        }

        public static ScreenInfo[] GetScreens()
        {

            var count = GetScreenCount();
            var screens = new ScreenInfo[count];
            for (int i = 0; i < count; i++)
            {
                SDL_DisplayMode displayMode;
                Rectangle bounds;
                unsafe
                {
                    Sdl2Native.SDL_GetCurrentDisplayMode(i, &displayMode);
                    Sdl2Native.SDL_GetDisplayBounds(i, &bounds);
                }

                screens[i] = new ScreenInfo(bounds, displayMode.refresh_rate);
            }
            return screens;
        }

        public static ScreenInfo GetVirtualScreen()
        {
            var bounds = new Rectangle();
            int refreshRate = int.MaxValue;

            var screens = GetScreens();
            var xStarts = new List<int>(screens.Length);
            var yStarts = new List<int>(screens.Length);

            for (int i = 0; i < screens.Length; i++)
            {
                ScreenInfo screen = screens[i];

                if(!xStarts.Any(xs => xs == screen.Bounds.X))
                {
                    xStarts.Add(screen.Bounds.X);
                    bounds.Width += screen.Bounds.Width;
                }
                if (!yStarts.Any(ys => ys == screen.Bounds.Y))
                {
                    yStarts.Add(screen.Bounds.Y);
                    bounds.Height += screen.Bounds.Height;
                }

                if(screen.RefreshRate < refreshRate)
                {
                    refreshRate = screen.RefreshRate;
                }
            }
            return new ScreenInfo(bounds, refreshRate);
        }

        public Rectangle Bounds { get; private set; }
        public int RefreshRate { get; private set; }

        internal ScreenInfo(Rectangle bounds, int refreshRate)
        {
            Bounds = bounds;
            RefreshRate = refreshRate;
        }
    }
}
