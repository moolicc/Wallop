using IronPython.Hosting;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting.Hosting;
using Wallop.DSLExtension.Scripting;
using IronPy = IronPython;

namespace Scripting.IronPython
{

    public class PythonScriptEngineProvider : IScriptEngineProvider
    {
        public string Name => "Iron Python";

        public IScriptContext CreateContext()
        {
            return new PythonScriptContext();
        }

        public IScriptEngine CreateScriptEngine(IEnumerable<KeyValuePair<string, string>> args)
        {
            var engineArgs = args.Select(x => new KeyValuePair<string, object>(x.Key, x.Value));
            return new PythonScriptEngine(new Dictionary<string, object>(engineArgs));
        }

        public bool TryDeserialize(string value, string targetType, out object? result)
        {
            result = null;
            if(targetType == "number")
            {
                result = float.Parse(value);
            }
            return true;
        }
    }

    public class PythonScriptEngine : IScriptEngine
    {
        private PythonScriptContext? _scriptContext;
        private ScriptEngine _pyEngine;
        private ScriptScope _pyScope;
        private int _lastLine;

        public PythonScriptEngine(Dictionary<string, object> args)
        {
            _pyEngine = Python.CreateEngine(args);
            _pyScope = _pyEngine.CreateScope();
            _pyScope.ImportModule("clr");
            _pyEngine.Execute("import clr", _pyScope);
            _lastLine = -1;
            //_pyEngine.Runtime.SetTrace(OnTraceback);
        }


        public IScriptContext? GetAttachedScriptContext()
            => _scriptContext;

        public void AttachContext(IScriptContext context)
        {
            if (context is PythonScriptContext pyScriptContext)
            {
                _scriptContext = pyScriptContext;
                _scriptContext.SetScope(_pyScope);
                _scriptContext.SetEngine(_pyEngine);
            }
        }

        public void Execute(string script)
        {
            var source = _pyEngine.CreateScriptSourceFromString(script);
            source.Execute(_pyScope);
        }

        public void Panic()
        {
            throw new SystemExitException("Panicking");
        }

        public int GetLastLineExecuted()
        {
            return _lastLine;
        }

        private TracebackDelegate OnTraceback(TraceBackFrame frame, string result, object payload)
        {
            _lastLine = (frame.f_lineno as int?) ?? -1;

            return OnTraceback;
        }
    }
}