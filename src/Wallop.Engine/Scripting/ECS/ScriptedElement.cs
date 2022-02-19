using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Modules;
using Wallop.DSLExtension.Scripting;
using Wallop.Engine.ECS;
using Wallop.Engine.Settings;

namespace Wallop.Engine.Scripting.ECS
{
    public class ScriptedElement : IEcsMember
    {
        public const bool OPERATIONS_MULTITHREADED = false;

        public string Name { get; init; }
        public Module ModuleDeclaration { get; init; }
        public StoredModule StoredDefinition { get; init; }
        public IScriptEngine? ScriptEngine { get; private set; }

        public bool IsPanicState { get; private set; }

        public Action<ScriptedElement>? BeforeUpdateCallback;
        public Action<ScriptedElement>? AfterUpdateCallback;
        public Action<ScriptedElement>? BeforeDrawCallback;
        public Action<ScriptedElement>? AfterDrawCallback;
        public Action<ScriptedElement>? PanicCallback;

        private TaskHandler? _taskHandler;
        private ScriptPanicException? _panic;


        public ScriptedElement(string name, Module declaringModule, StoredModule storedModule)
        {
            Name = name;
            ModuleDeclaration = declaringModule;
            StoredDefinition = storedModule;
        }

        public void InitializeScript(IScriptEngine engine, string source)
        {
            _taskHandler = new TaskHandler(engine);
            ScriptEngine = engine;

            InvokeOnScriptThread(() => engine.Execute(source));
        }

        public async Task WaitForExecuteAsync()
        {
            await _taskHandler.OrThrow().WaitForEmptyAsync();
        }

        public void Shutdown()
        {
            BeforeUpdateCallback = null;
            AfterUpdateCallback = null;
            BeforeDrawCallback = null;
            AfterDrawCallback = null;
            OnShutdown();
            _taskHandler?.Terminate();
            _taskHandler = null;
        }

        public IScriptContext GetAttachedScriptContext()
        {
            return ScriptEngine.OrThrow("Actor not bound to a ScriptEngine.").GetAttachedScriptContext().OrThrow("No ScriptContext attached to actor.");
        }

        public void Update()
        {
            if(IsPanicState)
            {
                HandlePendingPanic();
                return;
            }

            if(BeforeUpdateCallback != null)
            {
                InvokeOnScriptThread(() => BeforeUpdateCallback(this));
            }

            try
            {
                InvokeScriptAction("update");
            }
            catch (Exception ex)
            { Panic(ex, false); }

            if (IsPanicState)
            {
                return;
            }

            if (AfterUpdateCallback != null)
            {
                InvokeOnScriptThread(() => AfterUpdateCallback(this));
            }
        }

        public void Draw()
        {
            if (IsPanicState)
            {
                HandlePendingPanic();
                return;
            }

            if (BeforeDrawCallback != null)
            {
                InvokeOnScriptThread(() => BeforeDrawCallback(this));
            }

            try
            {
                InvokeScriptAction("draw");
            }
            catch (Exception ex)
            { Panic(ex, false); }

            if (IsPanicState)
            {
                return;
            }

            if (AfterDrawCallback != null)
            {
                InvokeOnScriptThread(() => AfterDrawCallback(this));
            }
        }

        public void Panic(string reason, bool generatedByScript)
        {
            Panic(new ScriptPanicException(reason, this, generatedByScript));
        }

        public void Panic(Exception cause, bool generatedByScript)
        {
            if(cause is ScriptPanicException)
            {
                throw new InvalidOperationException($"Panic should not be called with an inner-exception cause of type {nameof(ScriptPanicException)}.");
            }
           Panic(new ScriptPanicException(cause.Message, this, generatedByScript, cause));
        }

        public void Panic(ScriptPanicException exception)
        {
            _panic = exception;
            IsPanicState = true;
            if (exception.GeneratedByScript)
            {
                ScriptEngine?.Panic();
            }
            else
            {
                HandlePendingPanic();
            }
        }


        protected virtual void OnShutdown()
        { }

        private void InvokeOnScriptThread(Action action)
        {
            if (OPERATIONS_MULTITHREADED)
            {
                _taskHandler?.OrThrow().EnqueueAction(action);
            }
            else
            {
                _taskHandler?.OrThrow().RunAction(action);
            }
        }

        private void InvokeScriptAction(string actionName)
        {
            if (OPERATIONS_MULTITHREADED)
            {
                _taskHandler?.OrThrow().EnqueueAction<Action>(actionName, a => a());
            }
            else
            {
                _taskHandler?.OrThrow().RunAction<Action>(actionName, a => a());
            }
        }

        private void HandlePendingPanic()
        {
            if(_panic == null)
            {
                return;
            }

            EngineLog.For<ScriptedElement>().Error(_panic, "ECS element is panicking! Clearing element state. Last line executed: {line}, Panic Exception: {exc}", ScriptEngine?.GetLastLineExecuted(), _panic);
            Shutdown();
            PanicCallback?.Invoke(this);

            _panic = null;
        }
    }
}
