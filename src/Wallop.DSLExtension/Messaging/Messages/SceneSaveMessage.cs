namespace Wallop.Shared.Messaging.Messages
{
    public readonly record struct SceneSaveMessage(int? Options = null, string? Location = null);
}