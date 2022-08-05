using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Modules;
using Wallop.Shared.Scripting;
using Wallop.Shared.Types.Plugin;

namespace Wallop.Types.Plugins.EndPoints
{
    internal class ScriptInjectEndPoint : EndPointBase, IInjectScriptContextEndPoint
    {
        public Module CurrentModule { get; private set; }

        public IScriptContext Context { get; private set; }

        public ScriptInjectEndPoint(Messaging.Messenger messages, Module currentModule, IScriptContext currentContext)
            : base(messages)
        {
            CurrentModule = currentModule;
            Context = currentContext;
        }
    }
}
