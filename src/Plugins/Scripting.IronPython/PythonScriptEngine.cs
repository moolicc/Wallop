using IronPython.Hosting;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting.Hosting;
using Wallop.Shared.ECS.ActorQuerying.FilterMachine;
using Wallop.Shared.Scripting;
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
            var engineArgs = ScriptEngineServices.CastArgs(args, new Dictionary<string, Type>
            {
                { "debug", typeof(bool) }
            });

            return new PythonScriptEngine(new Dictionary<string, object>(engineArgs));
        }
    }

    public class PythonScriptEngine : IScriptEngine
    {
        private PythonScriptContext? _scriptContext;
        private ScriptEngine _pyEngine;
        private ScriptScope _pyScope;
        private EngineState _state;

        public PythonScriptEngine(Dictionary<string, object> args)
        {
            _pyEngine = Python.CreateEngine(args);
            _pyScope = _pyEngine.CreateScope();
            _pyScope.ImportModule("clr");
            _pyEngine.Execute("import clr", _pyScope);

            _state = new EngineState();
            _state.ReportedStatus = "<Not implemented>";

            if (args.TryGetValue("debug", out var debug) && debug is bool b && b)
            {
                _pyEngine.Runtime.SetTrace(OnTraceback);
            }
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

        public void SaveState(IScriptContext context, IDictionary<string, string> storage)
        {
            if (context is PythonScriptContext pyScriptContext)
            {
                var scope = pyScriptContext.GetAddedValues();
            }
        }

        public void LoadState(IScriptContext context, IReadOnlyDictionary<string, string> storage)
        {
            throw new NotImplementedException();
        }

        public void ExecuteRaw(string script)
        {
            var source = _pyEngine.CreateScriptSourceFromString(script);
            source.Execute(_pyScope);
        }

        public void ExecuteFile(string scriptFile)
        {
            var source = _pyEngine.CreateScriptSourceFromFile(scriptFile);
            source.Execute(_pyScope);
        }


        public void Panic()
        {
        }

        public EngineState GetState()
        {
            return _state;
        }

        private TracebackDelegate OnTraceback(TraceBackFrame frame, string action, object payload)
        {
            const string ACTION_ENTER = "call";
            const string ACTION_LINE = "line";
            const string ACTION_EXIT = "return";
            const string ACTION_ERROR = "exception";

            int lineNo = -1;
            if(frame.f_lineno is int i)
            {
                lineNo = i;
            }

            if(action == ACTION_ENTER)
            {
                State_EnterMethod(frame.f_code.co_filename, frame.f_code.co_name, lineNo);
            }
            else if(action == ACTION_LINE)
            {
                State_LineChange(lineNo);
            }
            else if(action == ACTION_EXIT)
            {
                State_ExitMethod();
            }
            else if (action == ACTION_ERROR)
            {
                State_Error(lineNo);
            }

            return OnTraceback;
        }

        private void State_EnterMethod(string ns, string funcName, int lineNo)
        {
            var frame = new CallFrame(ns, funcName, lineNo, false);
            _state.CallStack.Add(frame);
        }

        private void State_LineChange(int lineNo)
        {
            // If we've reached a new line (not a return) and we're in an error state, then we must have caught the exception.
            if (_state.CallStack[_state.CallStack.Count - 1].ExceptionState)
            {
                _state.CallStack[_state.CallStack.Count - 1] = _state.CallStack[_state.CallStack.Count - 1] with
                {
                    ExceptionState = false,
                    LineNumber = lineNo
                };
            }
            else
            {
                _state.CallStack[_state.CallStack.Count - 1] = _state.CallStack[_state.CallStack.Count - 1] with
                {
                    LineNumber = lineNo
                };
            }
        }

        private void State_ExitMethod()
        {
            if (!_state.CallStack[_state.CallStack.Count - 1].ExceptionState)
            {
                _state.CallStack.RemoveAt(_state.CallStack.Count - 1);
            }
        }

        private void State_Error(int lineNo)
        {
            // The provided line number will be the last line to have executed without error.
            _state.CallStack[_state.CallStack.Count - 1] = _state.CallStack[_state.CallStack.Count - 1] with
            {
                LineNumber = lineNo + 1,
                ExceptionState = true
            };
        }
    }
}