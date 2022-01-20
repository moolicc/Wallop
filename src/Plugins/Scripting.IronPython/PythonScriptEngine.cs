using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using IronPy = IronPython;

namespace Scripting.IronPython
{

    public class PythonScriptEngineProvider : Wallop.DSLExtension.Scripting.IScriptEngineProvider
    {
        public string Name => "Iron Python";

        public Wallop.DSLExtension.Scripting.IScriptEngine CreateScriptEngine(IEnumerable<KeyValuePair<string, string>> args)
        {
            var engineArgs = args.Select(x => new KeyValuePair<string, object>(x.Key, x.Value));
            return new PythonScriptEngine(new Dictionary<string, object>(engineArgs));
        }
    }

    public class PythonScriptEngine : Wallop.DSLExtension.Scripting.IScriptEngine
    {
        private ScriptEngine _pyEngine;
        private ScriptScope _pyScope;

        public PythonScriptEngine(Dictionary<string, object> args)
        {
            _pyEngine = Python.CreateEngine(args);
            _pyScope = _pyEngine.CreateScope();
            _pyScope.ImportModule("clr");
            _pyEngine.Execute("import clr", _pyScope);
        }

        public void Init(Wallop.DSLExtension.Scripting.ScriptContext context)
        {
            foreach (var reference in context.References)
            {
                _pyEngine.Execute($"clr.AddReference(\"{reference}\")", _pyScope);
            }

            foreach (var method in context.ExposedDelegates)
            {
                _pyScope.SetVariable(method.MemberName, method.Action);
            }

            foreach (var member in context.ExposedVariables)
            {
                _pyScope.SetVariable(member.MemberName, member.Value);
            }

            foreach (var import in context.Imports)
            {
                _pyEngine.Execute($"from {import.Assembly} import {import.Namespace}", _pyScope);
            }

        }

        public void Execute(string script)
        {
            var source = _pyEngine.CreateScriptSourceFromString(script);
            source.Execute(_pyScope);
        }

        public object GetValue(string memberName)
        {
            return _pyScope.GetVariable(memberName);
        }

        public T GetValue<T>(string memberName)
        {
            return _pyScope.GetVariable<T>(memberName);
        }

        public T GetDelegateAs<T>(string memberName)
        {
            return _pyScope.GetVariable<T>(memberName);
        }

    }
}