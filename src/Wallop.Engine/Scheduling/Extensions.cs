using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.Scripting;

namespace Wallop.Engine.Scheduling
{
    public static class Extensions
    {
        public static void QueueUpdateTaskSafe(this TaskHandler taskHandler, Scripting.ECS.ScriptedElement element, Action<object?> action, object? arg)
        {
            var newArg = (element, arg);
            taskHandler.QueueUpdateTask(element, (d) =>
            {
                if(d == null)
                {
                    throw new NullReferenceException();
                }
                var data = ((Scripting.ECS.ScriptedElement element, object? arg))d;
                if(!data.element.IsPanicState)
                {
                    action(data.arg);
                }
            }, newArg);
        }

        public static void QueueDrawTaskSafe(this TaskHandler taskHandler, Scripting.ECS.ScriptedElement element, Action<object?> action, object? arg)
        {
            var newArg = (element, arg);
            taskHandler.QueueDrawTask(element, (d) =>
            {
                if (d == null)
                {
                    throw new NullReferenceException();
                }
                var data = ((Scripting.ECS.ScriptedElement element, object? arg))d;
                if (!data.element.IsPanicState)
                {
                    action(data.arg);
                }
            }, newArg);
        }
    }
}
