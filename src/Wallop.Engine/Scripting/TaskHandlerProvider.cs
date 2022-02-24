using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Scripting;

namespace Wallop.Engine.Scripting
{
    public enum ThreadingPolicy
    {
        SingleThread,
        Multithread,
    }

    public enum TaskOperations
    {
        Update,
        Draw,
    }

    public class TaskHandlerProvider
    {
        public ThreadingPolicy UpdatePolicy { get; private set; }
        public ThreadingPolicy DrawPolicy { get; private set; }

        public Action DrawFirstRunCallback;

        private Dictionary<ECS.ScriptedElement, TaskHandler> _updateHandlers;
        private Dictionary<ECS.ScriptedElement, TaskHandler> _drawHandlers;

        public TaskHandlerProvider(ThreadingPolicy updatePolicy, ThreadingPolicy drawPolicy, Action drawFirstRunCallback)
        {
            UpdatePolicy = updatePolicy;
            DrawPolicy = drawPolicy;
            DrawFirstRunCallback = drawFirstRunCallback;

            _updateHandlers = new Dictionary<ECS.ScriptedElement, TaskHandler>();
            _drawHandlers = new Dictionary<ECS.ScriptedElement, TaskHandler>();
        }

        public void CleanupHandlers(ECS.ScriptedElement element)
        {
            EngineLog.For<TaskHandlerProvider>().Info("Cleaning up handlers for element {element}...", element.Name);

            var handler = _updateHandlers[element];
            _updateHandlers.Remove(element);
            if(UpdatePolicy != ThreadingPolicy.SingleThread)
            {
                EngineLog.For<TaskHandlerProvider>().Info("Terminating update handler on thread {thread}...", handler.BackingThread.Name);
                handler.Terminate();
            }

            handler = _drawHandlers[element];
            _drawHandlers.Remove(element);
            if (DrawPolicy != ThreadingPolicy.SingleThread)
            {
                EngineLog.For<TaskHandlerProvider>().Info("Terminating draw handler on thread {thread}...", handler.BackingThread.Name);
                handler.Terminate();
            }
        }

        public TaskHandler GetUpdateHandler(ECS.ScriptedElement element)
        {
            if (!_updateHandlers.TryGetValue(element, out var handler))
            {
                if (UpdatePolicy == ThreadingPolicy.SingleThread && _updateHandlers.Count > 0)
                {
                    handler = _updateHandlers.First().Value;
                }
                else
                {
                    handler = new TaskHandler(false);
                    EngineLog.For<TaskHandlerProvider>().Info("Creating update handler for {element} on thread {thread}", element.Name, handler.BackingThread.Name);
                }

                _updateHandlers.Add(element, handler);
            }

            return handler;
        }

        public TaskHandler GetDrawHandler(ECS.ScriptedElement element)
        {
            if (!_drawHandlers.TryGetValue(element, out var handler))
            {
                if (DrawPolicy == ThreadingPolicy.SingleThread && _drawHandlers.Count > 0)
                {
                    handler = _drawHandlers.First().Value;
                }
                else
                {
                    handler = new TaskHandler(true);
                    EngineLog.For<TaskHandlerProvider>().Info("Creating draw handler for {element} on thread {thread}", element.Name, handler.BackingThread.Name);
                    handler.QueueTask(null, _ => DrawFirstRunCallback());
                }

                _drawHandlers.Add(element, handler);
            }

            return handler;
        }
    }
}
