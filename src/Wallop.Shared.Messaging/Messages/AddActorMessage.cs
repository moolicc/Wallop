
namespace Wallop.Shared.Messaging.Messages
{
    public readonly record struct AddActorMessage(string ActorName, string? Scene, string? Layout, string BasedOnModule, IEnumerable<KeyValuePair<string, string?>>? ModuleSettings = null);
}
