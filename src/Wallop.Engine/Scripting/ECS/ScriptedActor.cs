using Wallop.DSLExtension.Modules;
using Wallop.Engine.ECS;
using Wallop.Engine.SceneManagement;
using Wallop.Engine.Settings;

namespace Wallop.Engine.Scripting.ECS
{
    internal class ScriptedActor : ScriptedEcsComponent, IActor
    {
        public const char NAMESPACE_DELIMITER = ':';

        public string Id { get; private set; }
        public Layout OwningLayout { get; private set; }
        public List<Component> Components => new List<Component>();

        public ScriptedActor(Module declaringModule, StoredModule storedModule, Layout owningLayout)
            : base(storedModule.InstanceName, declaringModule, storedModule)
        {
            Id = owningLayout.Name + NAMESPACE_DELIMITER + storedModule.InstanceName;
            OwningLayout = owningLayout;
        }
    }
}
