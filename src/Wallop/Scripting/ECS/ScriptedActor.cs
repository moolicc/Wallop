using Wallop.ECS;
using Wallop.Settings;
using Wallop.Shared.ECS;
using Wallop.Shared.Modules;
using Wallop.Shared.Scripting;

namespace Wallop.Scripting.ECS
{
    public class ScriptedActor : ScriptedElement, IActor
    {
        public const char NAMESPACE_DELIMITER = ':';

        public string Id { get; private set; }
        public List<object> Components { get; private set; }

        public Layout OwningLayout
        {
            get
            {
                if(_owningLayout == null)
                {
                    throw new InvalidOperationException("Actor not yet added to layout.");
                }
                return _owningLayout;
            }
        }

        private Layout? _owningLayout;

        public ScriptedActor(Module declaringModule, StoredModule storedModule)
            : base(storedModule.InstanceName, declaringModule, storedModule)
        {
            Id = "";
            Components = new List<object>();
        }

        public void AddedToLayout(ILayout owner)
        {
            if (_owningLayout != null || owner is not Layout)
            {
                // TODO: Error
            }
            _owningLayout = (Layout)owner;
            Id = owner.Name + NAMESPACE_DELIMITER + StoredDefinition.InstanceName;
        }

        protected override void OnShutdown()
        {
            foreach (var component in Components)
            {
                if (component is BindableType bindable)
                {
                    bindable.Cleanup();
                }
            }
            base.OnShutdown();
        }
    }
}
