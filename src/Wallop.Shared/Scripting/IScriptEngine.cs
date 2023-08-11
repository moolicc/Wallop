using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Scripting
{
    public interface IScriptEngine
    {
        public void AttachContext(IScriptContext context);

        public void SaveState(IScriptContext context, IDictionary<string, string> storage);
        public void LoadState(IScriptContext context, IReadOnlyDictionary<string, string> storage);

        public IScriptContext? GetAttachedScriptContext();

        public void ExecuteRaw(string script);
        public void ExecuteFile(string scriptFile);

        public EngineState GetState();
        public void Panic();
    }
}
