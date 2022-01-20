using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.DSLExtension.Scripting
{
    public interface IScriptEngine
    {
        public void Init(ScriptContext context);
        public void Execute(string script);
        public T GetDelegateAs<T>(string memberName);
        public object? GetValue(string memberName);
        public T? GetValue<T>(string memberName);
    }
}
