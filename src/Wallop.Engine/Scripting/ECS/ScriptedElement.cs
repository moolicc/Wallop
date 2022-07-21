using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Modules;
using Wallop.DSLExtension.Scripting;
using Wallop.Engine.ECS;
using Wallop.Engine.Scheduling;
using Wallop.Engine.SceneManagement;

namespace Wallop.Engine.Scripting.ECS
{
    public class ScriptedElement : IEcsMember
    {
        public const bool OPERATIONS_MULTITHREADED = true;

        public string Name { get; init; }
        public Module ModuleDeclaration { get; init; }
        public StoredModule StoredDefinition { get; init; }
        public IScriptEngine? ScriptEngine { get; private set; }
        public Dictionary<string, string> Config { get; private set; }

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
            Config = new Dictionary<string, string>(storedModule.Config.Select(v => new KeyValuePair<string, string>(v.Name, v.Value)));
        }

        public void InitializeScript(TaskHandler taskHandler, IScriptEngine engine, string source)
        {
            _taskHandler = taskHandler;
            ScriptEngine = engine;
            _taskHandler.QueueUpdateTask(this, (src) => engine.Execute(src.OrThrow().ToString().OrThrow()), source);
        }

        public async Task WaitForExecuteAsync()
        {
            if(IsPanicState)
            {
                return;
            }

            //await _taskProvider.OrThrow().GetUpdateHandler(this).WaitForEmptyAsync();
            //await _taskProvider.OrThrow().GetDrawHandler(this).WaitForEmptyAsync();
            return;
        }

        public void Shutdown()
        {
            EngineLog.For<ScriptedElement>().Info("Shutting down element {element}...", Name);
            BeforeUpdateCallback = null;
            AfterUpdateCallback = null;
            BeforeDrawCallback = null;
            AfterDrawCallback = null;
            OnShutdown();
            _taskHandler.OrThrow().Cleanup(this);
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

            if (BeforeUpdateCallback != null)
            {
                _taskHandler?.QueueUpdateTaskSafe(this, s => BeforeUpdateCallback((ScriptedElement)s), this);
            }

            try
            {
                RunScriptAction(true, "update");
            }
            catch (Exception ex)
            { Panic(ex, false); }

            if (IsPanicState)
            {
                HandlePendingPanic();
                return;
            }

            if (AfterUpdateCallback != null)
            {
                _taskHandler?.QueueUpdateTaskSafe(this, s => AfterUpdateCallback((ScriptedElement)s), this);
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
                _taskHandler?.QueueDrawTaskSafe(this, s => BeforeDrawCallback((ScriptedElement)s), this);
            }

            try
            {
                RunScriptAction(false, "draw");
            }
            catch (Exception ex)
            { Panic(ex, false); }

            if (IsPanicState)
            {
                HandlePendingPanic();
                return;
            }

            if (AfterDrawCallback != null)
            {
                _taskHandler?.QueueDrawTaskSafe(this, s => AfterDrawCallback((ScriptedElement)s), this);
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

        private void RunScriptAction(bool update, string actionName)
        {
            var action = new Action<object?>((values) =>
            {
                try
                {
                    if(IsPanicState)
                    {
                        return;
                    }
                    if(values == null)
                    {
                        throw new NullReferenceException();
                    }
                    var tup = ((IScriptContext, string))values;
                    tup.Item1.GetDelegateAs<Action>(tup.Item2)();
                }
                catch (Exception ex)
                {
                    Panic(ex, true);
                }
            });

            if(update)
            {
                _taskHandler?.QueueUpdateTask(this, action, (GetAttachedScriptContext(), actionName));
            }
            else
            {
                _taskHandler?.QueueDrawTask(this, action, (GetAttachedScriptContext(), actionName));
            }
        }

        private void HandlePendingPanic()
        {
            if(_panic == null)
            {
                return;
            }

            EngineLog.For<ScriptedElement>().Error(_panic, "ECS element {element} is panicking! {reason} Last line executed: {line}, Panic Exception: {exc}", Name, _panic.PanicReason, ScriptEngine?.GetLastLineExecuted(), _panic);
            Shutdown();
            PanicCallback?.Invoke(this);

            _panic = null;
        }
    }
}
