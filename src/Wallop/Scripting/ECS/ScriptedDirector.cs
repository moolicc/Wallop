using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.SceneManagement;
using Wallop.Shared.ECS;
using Wallop.Shared.Modules;

namespace Wallop.Scripting.ECS
{
    public class ScriptedDirector : ScriptedElement, IDirector
    {
        public ScriptedDirector(Module declaringModule, StoredModule storedModule)
            : base(storedModule.InstanceName, declaringModule, storedModule)
        {
        }
    }
}
