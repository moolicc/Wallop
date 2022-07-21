using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Modules;
using Wallop.SceneManagement;

namespace Wallop.Scripting.ECS
{
    public class ScriptedDirector : ScriptedElement, Wallop.ECS.IDirector
    {
        public ScriptedDirector(Module declaringModule, StoredModule storedModule)
            : base(storedModule.InstanceName, declaringModule, storedModule)
        {
        }
    }
}
