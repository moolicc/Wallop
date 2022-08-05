using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Modules;
using Wallop.Shared.Scripting;

namespace Wallop.Shared.Types.Plugin
{
    public interface IInjectScriptContextEndPoint
    {
        public Module CurrentModule { get; }
        public IScriptContext Context { get; }
    }
}
