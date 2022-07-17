using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Modules;
using Wallop.DSLExtension.Scripting;

namespace Wallop.Engine.Types.Plugins.EndPoints
{
    internal class ScriptInjectEndPoint : EndPointBase, DSLExtension.Types.Plugin.IInjectScriptContextEndPoint
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
