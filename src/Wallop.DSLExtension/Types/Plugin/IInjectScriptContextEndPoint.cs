using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Modules;
using Wallop.DSLExtension.Scripting;

namespace Wallop.DSLExtension.Types.Plugin
{
    public interface IInjectScriptContextEndPoint
    {
        public Module CurrentModule { get; }
        public ScriptContext Context { get; }
    }
}
