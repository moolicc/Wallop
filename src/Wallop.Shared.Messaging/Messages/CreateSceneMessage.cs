
namespace Wallop.Shared.Messaging.Messages
{
    // TODO: Scene cloning
    public readonly record struct CreateSceneMessage(string NewSceneName, string BasedOnScene);
}
