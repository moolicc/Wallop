using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Messaging;
using Wallop.Shared.Messaging.Messages;

namespace Wallop.Types.Plugins.EndPoints
{
    public class OverlayerEndPoint : EndPointBase
    {
        public IWindow Window { get; private set; }
        public nint WindowHandle => Window.Handle;

        internal OverlayerEndPoint(Messenger messages, IWindow window)
            : base(messages)
        {
            Window = window;
        }
    }
}
