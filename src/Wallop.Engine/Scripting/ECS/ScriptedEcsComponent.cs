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
    public class ScriptedEcsComponent : IEcsMember
    {
        public const bool OPERATIONS_MULTITHREADED = false;

        public string Name { get; init; }
        public Module ModuleDeclaration { get; init; }
        public StoredModule StoredDefinition { get; init; }
        public IScriptEngine? ScriptEngine { get; private set; }

        public Action<ScriptedEcsComponent> BeforeUpdateCallback;
        public Action<ScriptedEcsComponent> AfterUpdateCallback;
        public Action<ScriptedEcsComponent> BeforeDrawCallback;

        public Action<ScriptedEcsComponent> AfterDrawCallback;

        private TaskHandler? TaskHandler;


        public ScriptedEcsComponent(string name, Module declaringModule, StoredModule storedModule)
        {
            Name = name;
            ModuleDeclaration = declaringModule;
            StoredDefinition = storedModule;
        }

        public void InitializeScript(IScriptEngine engine, string source)
        {
            TaskHandler = new TaskHandler(engine);
            ScriptEngine = engine;

            InvokeOnScriptThread(() => engine.Execute(source));
        }

        public async Task WaitForExecuteAsync()
        {
            await TaskHandler.OrThrow().WaitForEmptyAsync();
        }

        public void Shutdown()
        {
            TaskHandler.Terminate();
        }

        public IScriptContext GetAttachedScriptContext()
        {
            return ScriptEngine.OrThrow("Actor not bound to a ScriptEngine.").GetAttachedScriptContext().OrThrow("No ScriptContext attached to actor.");
        }

        public void Update()
        {
            if(BeforeUpdateCallback != null)
            {
                InvokeOnScriptThread(() => BeforeUpdateCallback(this));
            }
            InvokeScriptAction("update");
            if (AfterUpdateCallback != null)
            {
                InvokeOnScriptThread(() => AfterUpdateCallback(this));
            }
        }

        public void Draw()
        {
            if (BeforeDrawCallback != null)
            {
                InvokeOnScriptThread(() => BeforeDrawCallback(this));
            }
            InvokeScriptAction("draw");
            if (AfterDrawCallback != null)
            {
                InvokeOnScriptThread(() => AfterDrawCallback(this));
            }
        }

        private void InvokeOnScriptThread(Action action)
        {
            if (OPERATIONS_MULTITHREADED)
            {
                TaskHandler.OrThrow().EnqueueAction(action);
            }
            else
            {
                TaskHandler.OrThrow().RunAction(action);
            }
        }

        private void InvokeScriptAction(string actionName)
        {
            if (OPERATIONS_MULTITHREADED)
            {
                TaskHandler.OrThrow().EnqueueAction<Action>(actionName, a => a());
            }
            else
            {
                TaskHandler.OrThrow().RunAction<Action>(actionName, a => a());
            }
        }
    }
}
