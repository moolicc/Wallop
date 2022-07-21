using Wallop.SceneManagement.Serialization;

namespace Wallop.Messaging.Messages
{
    public readonly record struct SceneSaveMessage(SettingsSaveOptions Options, string Location);
}