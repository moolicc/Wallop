using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Modules;
using Wallop.Engine.Settings;

namespace Wallop.Engine.Scripting.ECS
{
    public class ScriptedDirector : ScriptedEcsComponent, Engine.ECS.IDirector
    {
        public ScriptedDirector(Module declaringModule, StoredModule storedModule)
            : base(storedModule.InstanceName, declaringModule, storedModule)
        {
        }
    }
}
