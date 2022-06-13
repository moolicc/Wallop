﻿using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Types.Plugins.EndPoints
{
    public class OverlayerEndPoint : EndPointBase
    {
        public IWindow Window { get; private set; }
        public nint WindowHandle => Window.Handle;

        internal OverlayerEndPoint(IWindow window)
        {
            Window = window;
        }
    }
}