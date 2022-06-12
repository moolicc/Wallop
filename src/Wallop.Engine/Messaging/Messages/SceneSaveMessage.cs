using Wallop.Engine.SceneManagement.Serialization;

namespace Wallop.Engine.Messaging.Messages
{
    public readonly record struct SceneSaveMessage(SettingsSaveOptions Options, string Location);
}