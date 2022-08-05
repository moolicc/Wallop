
namespace Wallop.Shared.Messaging.Messages
{
    public readonly record struct AddDirectorMessage(string DirectorName, string BasedOnModule, string Scene, IEnumerable<KeyValuePair<string, string>>? ModuleSettings = null);
}
