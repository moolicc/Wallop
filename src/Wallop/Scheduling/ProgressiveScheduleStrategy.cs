using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Scheduling
{
    public class ProgressiveScheduleStrategy : IScheduleStrategy
    {
        public double UtilizationFactor { get; set; }

        private Queue<ActionRun> _tasks;
        private List<ActionRun> _incomingTasks;

        private int _lastTask;

        public ProgressiveScheduleStrategy(double utilizationFactor)
        {
            UtilizationFactor = utilizationFactor;
            _tasks = new Queue<ActionRun>();
            _incomingTasks = new List<ActionRun>();
            _lastTask = -1;
        }

        public IEnumerable<ActionRun> GetScheduledActions()
        {
            var list = _tasks.ToList();
            list.AddRange(_incomingTasks);
            return list;
        }

        public void OnActionScheduled(ActionRun action)
        {
            _incomingTasks.Add(action);
        }

        public void OnTick()
        {
            RunModifications();
            if (_tasks.Count == 0)
            {
                return;
            }

            double tasksPerFrame = Math.Ceiling(_tasks.Count * UtilizationFactor);

            int i = 0;
            while(i <= tasksPerFrame && _tasks.TryDequeue(out var task))
            {
                task.Action(task.State);
                i++;
            }


            RunModifications();
        }

        private void RunModifications()
        {
            foreach (var mod in _incomingTasks)
            {
                _tasks.Enqueue(mod);
            }

            _incomingTasks.Clear();
            if (_lastTask >= _tasks.Count())
            {
                _lastTask = -1;
            }
        }
    }
}
