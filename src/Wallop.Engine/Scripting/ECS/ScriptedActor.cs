using Wallop.DSLExtension.Modules;
using Wallop.Engine.ECS;
using Wallop.Engine.Settings;

namespace Wallop.Engine.Scripting.ECS
{
    internal class ScriptedActor : ScriptedEcsComponent, IActor
    {
        public List<Component> Components => new List<Component>();

        public ScriptedActor(Module declaringModule, StoredModule storedModule)
            : base(storedModule.InstanceName, declaringModule, storedModule)
        {
        }
    }
}
