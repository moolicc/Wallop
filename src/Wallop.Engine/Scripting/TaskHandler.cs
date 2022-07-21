using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.Scripting.ECS;

namespace Wallop.Engine.Scripting
{
    public enum ThreadingPolicy
    {
        SingleThread,
        MultiThread,
        Progressive,
        MultiThreadProgressive
    }

    public class TaskHandler
    {
        public ThreadingPolicy UpdatePolicy
        {
            get => _updatePolicy;
            set
            {
                if (_updatePolicy == value)
                {
                    return;
                }
                _sharedUpdateKey = null;
                _updatePolicy = value;

                RecreateMultiScheduleConfiguration_Update();
            }
        }
        public ThreadingPolicy DrawPolicy
        {
            get => _drawPolicy;
            set
            {
                if(_drawPolicy == value)
                {
                    return;
                }
                _sharedDrawKey = null;
                _drawPolicy = value;

                RecreateMultiScheduleConfiguration_Draw();
            }
        }
        public double ProgressiveUtilizationFactor { get; set; }

        private ThreadingPolicy _updatePolicy;
        private ThreadingPolicy _drawPolicy;
        private Scheduling.MultiScheduler<ScriptedElement> _updateScheduler;
        private Scheduling.MultiScheduler<ScriptedElement> _drawScheduler;

        private ScriptedElement? _sharedUpdateKey;
        private ScriptedElement? _sharedDrawKey;

        public TaskHandler(ThreadingPolicy updatePolicy, ThreadingPolicy drawPolicy)
        {
            ProgressiveUtilizationFactor = 0.5;

            _updatePolicy = updatePolicy;
            _drawPolicy = drawPolicy;

            _updateScheduler = new Scheduling.MultiScheduler<ScriptedElement>(CreateScheduleStrategy_Update);
            _drawScheduler = new Scheduling.MultiScheduler<ScriptedElement>(CreateScheduleStrategy_Draw);

            _sharedUpdateKey = null;
            _sharedDrawKey = null;
        }


        public void QueueUpdateTask(ECS.ScriptedElement element, Action<object?> action, object? arg)
        {
            if(_sharedUpdateKey == null)
            {
                if (UpdatePolicy == ThreadingPolicy.SingleThread || UpdatePolicy == ThreadingPolicy.Progressive)
                {
                    _sharedUpdateKey = element;
                }
                _updateScheduler.Schedule(element, new Scheduling.ActionRun(action, arg));
            }
            else
            {
                _updateScheduler.Schedule(_sharedUpdateKey, new Scheduling.ActionRun(action, arg));
            }
        }

        public void QueueDrawTask(ECS.ScriptedElement element, Action<object?> action, object? arg)
        {
            if (_sharedDrawKey == null)
            {
                if (DrawPolicy == ThreadingPolicy.SingleThread || DrawPolicy == ThreadingPolicy.Progressive)
                {
                    _sharedDrawKey = element;
                }
                _drawScheduler.Schedule(element, new Scheduling.ActionRun(action, arg));
            }
            else
            {
                _drawScheduler.Schedule(_sharedDrawKey, new Scheduling.ActionRun(action, arg));
            }
        }

        public void Cleanup(ScriptedElement scriptedElement)
        {
            if (_sharedUpdateKey == scriptedElement)
            {
                _sharedUpdateKey = null;
            }
            if(_sharedDrawKey == scriptedElement)
            {
                _sharedDrawKey = null;
            }

            if (UpdatePolicy != ThreadingPolicy.SingleThread && UpdatePolicy != ThreadingPolicy.Progressive)
            {
                _updateScheduler.RemoveStrategy(scriptedElement);
            }
            if (DrawPolicy != ThreadingPolicy.SingleThread && DrawPolicy != ThreadingPolicy.Progressive)
            {
                _drawScheduler.RemoveStrategy(scriptedElement);
            }
        }

        public void OnUpdate()
        {
            _updateScheduler.TickAll();
        }

        public void OnDraw()
        {
            _drawScheduler.TickAll();
        }


        private Scheduling.IScheduleStrategy CreateScheduleStrategy_Update(ECS.ScriptedElement element)
        {
            switch (UpdatePolicy)
            {
                case ThreadingPolicy.SingleThread:
                case ThreadingPolicy.MultiThread:
                    return new Scheduling.ImmediateScheduleStrategy();
                case ThreadingPolicy.Progressive:
                case ThreadingPolicy.MultiThreadProgressive:
                    return new Scheduling.ProgressiveScheduleStrategy(ProgressiveUtilizationFactor);
                default:
                    break;
            }

            throw new InvalidOperationException("Invalid UpdatePolicy.");
        }

        private Scheduling.IScheduleStrategy CreateScheduleStrategy_Draw(ECS.ScriptedElement element)
        {
            switch (DrawPolicy)
            {
                case ThreadingPolicy.SingleThread:
                case ThreadingPolicy.MultiThread:
                    return new Scheduling.ImmediateScheduleStrategy();
                case ThreadingPolicy.Progressive:
                case ThreadingPolicy.MultiThreadProgressive:
                    return new Scheduling.ProgressiveScheduleStrategy(ProgressiveUtilizationFactor);
                default:
                    break;
            }

            throw new InvalidOperationException("Invalid DrawPolicy.");
        }

        private void RecreateMultiScheduleConfiguration_Update()
        {
            _sharedUpdateKey = null;

            _updateScheduler.RecreateStrategies();
            _updateScheduler.MultithreadTicks = (UpdatePolicy == ThreadingPolicy.MultiThread || UpdatePolicy == ThreadingPolicy.MultiThreadProgressive);
        }

        private void RecreateMultiScheduleConfiguration_Draw()
        {
            _sharedDrawKey = null;

            _drawScheduler.RecreateStrategies();
            _drawScheduler.MultithreadTicks = (DrawPolicy == ThreadingPolicy.MultiThread || DrawPolicy == ThreadingPolicy.MultiThreadProgressive);
        }
    }
}
