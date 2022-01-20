using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Modules;
using Wallop.Engine.Settings;

namespace Wallop.Engine.Scripting.ECS
{
    internal class ScriptedDirector : Engine.ECS.Director
    {
        public Module ControllingModule { get; private set; }
        public StoredModule StoredDefinition { get; private set; }

        public ScriptedDirector(Module controllingModule, StoredModule storedDefinition)
        {
            ControllingModule = controllingModule;
            StoredDefinition = storedDefinition;
        }


        public override void Update()
        {
            // ScriptEngine.GetDelegate("update").DynamicInvoke();
        }

        public override void Draw()
        {
            // ScriptEngine.GetDelegate("draw").DynamicInvoke();
        }
    }
}
