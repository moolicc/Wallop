namespace Wallop.Shared.Messaging.Messages
{
    public readonly record struct SceneSettingsMessage(
        string? PackageSearchDirectory = null,
        string? DefaultSceneName = null,
        string? SelectedScene = null,
        string[]? ScenePreloads = null,
        int? UpdatePolicy = null,
        int? DrawPolicy = null);
}
