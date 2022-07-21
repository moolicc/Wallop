using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Scheduling
{
    public class ImmediateScheduleStrategy : IScheduleStrategy
    {
        private ConcurrentQueue<ActionRun> _actionQueue;

        public ImmediateScheduleStrategy()
        {
            _actionQueue = new ConcurrentQueue<ActionRun>();
        }

        public IEnumerable<ActionRun> GetScheduledActions()
        {
            return _actionQueue.ToArray();
        }

        public void OnActionScheduled(ActionRun action)
        {
            _actionQueue.Enqueue(action);
        }

        public void OnTick()
        {
            while (_actionQueue.TryDequeue(out var action))
            {
                action.Action(action.State);
            }
        }
    }
}
