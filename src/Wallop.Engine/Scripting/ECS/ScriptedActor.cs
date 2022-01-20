using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Modules;
using Wallop.DSLExtension.Scripting;
using Wallop.Engine.Settings;

namespace Wallop.Engine.Scripting.ECS
{
    internal class ScriptedActor : Engine.ECS.Actor
    {
        public Module ControllingModule { get; private set; }
        public StoredModule StoredDefinition { get; private set; }

        public ScriptedActorRunner? ActorRunner { get; set; }


        public ScriptedActor(Module controllingModule, StoredModule storedDefinition)
            : base(storedDefinition.InstanceName)
        {
            ControllingModule = controllingModule;
            StoredDefinition = storedDefinition;
        }

        public override void Update()
        {
            if (ActorRunner == null)
            {
                throw new InvalidOperationException();
            }
            ActorRunner.InvokeUpdateFor(Name);
        }

        public override void Draw()
        {
            if (ActorRunner == null)
            {
                throw new InvalidOperationException();
            }
            ActorRunner.InvokeRenderFor(Name);
        }
    }
}
