
namespace Wallop.Shared.Messaging.Messages
{
    public readonly record struct ReloadModuleMessage(string ModuleId, bool keepState);
}
