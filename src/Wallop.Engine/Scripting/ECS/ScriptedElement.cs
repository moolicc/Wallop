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
        public const bool OPERATIONS_MULTITHREADED = true;

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

        private TaskHandlerProvider? _taskProvider;
        private ScriptPanicException? _panic;


        public ScriptedElement(string name, Module declaringModule, StoredModule storedModule)
        {
            Name = name;
            ModuleDeclaration = declaringModule;
            StoredDefinition = storedModule;

        }

        public void InitializeScript(TaskHandlerProvider handlerProvider, IScriptEngine engine, string source)
        {
            _taskProvider = handlerProvider;
            ScriptEngine = engine;
            _taskProvider.GetDrawHandler(this).QueueTask(source, (src)
                => engine.Execute(src.OrThrow().ToString().OrThrow()));
        }

        public async Task WaitForExecuteAsync()
        {
            await _taskProvider.OrThrow().GetUpdateHandler(this).WaitForEmptyAsync();
            await _taskProvider.OrThrow().GetDrawHandler(this).WaitForEmptyAsync();
        }

        public void Shutdown()
        {
            BeforeUpdateCallback = null;
            AfterUpdateCallback = null;
            BeforeDrawCallback = null;
            AfterDrawCallback = null;
            OnShutdown();
            _taskProvider.OrThrow().CleanupHandlers(this);
        }

        public IScriptContext GetAttachedScriptContext()
        {
            return ScriptEngine.OrThrow("Element not bound to a ScriptEngine.").GetAttachedScriptContext().OrThrow("No ScriptContext attached to element.");
        }

        public void Update()
        {
            if(IsPanicState)
            {
                HandlePendingPanic();
                return;
            }

            var handler = _taskProvider.OrThrow().GetUpdateHandler(this);

            if (BeforeUpdateCallback != null)
            {
                handler.QueueTask(this, s => BeforeUpdateCallback((ScriptedElement)s));
            }

            try
            {
                RunScriptAction(handler, "update");
            }
            catch (Exception ex)
            { Panic(ex, false); }

            if (IsPanicState)
            {
                return;
            }

            if (AfterUpdateCallback != null)
            {
                handler.QueueTask(this, s => AfterUpdateCallback((ScriptedElement)s));
            }
        }

        public void Draw()
        {
            if (IsPanicState)
            {
                HandlePendingPanic();
                return;
            }

            var handler = _taskProvider.OrThrow().GetDrawHandler(this);
            if (BeforeDrawCallback != null)
            {
                handler.QueueTask(this, s => BeforeDrawCallback((ScriptedElement)s));
            }

            try
            {
                RunScriptAction(handler, "draw");
            }
            catch (Exception ex)
            { Panic(ex, false); }

            if (IsPanicState)
            {
                return;
            }

            if (AfterDrawCallback != null)
            {
                handler.QueueTask(this, s => AfterDrawCallback((ScriptedElement)s));
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
            if (!exception.GeneratedByScript)
            {
                HandlePendingPanic();
            }
        }


        protected virtual void OnShutdown()
        { }

        private void RunScriptAction(TaskHandler handler, string actionName)
        {
            handler.QueueTask((GetAttachedScriptContext(), actionName), (values) =>
            {
                var tup = ((IScriptContext, string))values;
                tup.Item1.GetDelegateAs<Action>(tup.Item2)();
            });
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
