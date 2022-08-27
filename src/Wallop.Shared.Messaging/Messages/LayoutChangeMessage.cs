using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Messaging.Messages
{
    public readonly record struct LayoutChangeMessage(string Layout, string? NewName, bool? NewActive, int? NewScreen, Vector2? NewRenderSize, Vector4? NewPresentationBounds);
}
