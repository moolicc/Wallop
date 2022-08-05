

namespace Wallop.Shared.Messaging.Messages
{
    public readonly record struct GraphicsMessage(
        int? WindowWidth = null,
        int? WindowHeight = null,
        bool? Overlay = null,
        int? WindowBorder = null,
        double? RefreshRate = null,
        bool? VSync = null);
}
