
using System.Numerics;

namespace Wallop.Shared.Messaging.Messages
{
    public readonly record struct AddLayoutMessage(string Name, string? Template, string? TargetScene, bool MakeActive, int ScreenIndex, Vector2? RenderSize, Vector4? PresentationBounds);
}
