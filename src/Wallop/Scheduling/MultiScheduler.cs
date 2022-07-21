using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Scripting.ECS;

namespace Wallop.Scheduling
{
    public class MultiScheduler<TKey>
        where TKey : notnull
    {
        public bool MultithreadTicks
        {
            get => _multithreadTicks;
            set
            {
                if (_multithreadTicks == value)
                {
                    return;
                }
                _multithreadTicks = value;

                if (value)
                {
                    SetupThreads();
                }
                else
                {
                    TeardownThreads();
                }
            }
        }

        private Dictionary<TKey, ScheduleInfo> _scheduleGroups;
        private List<KeyValuePair<TKey, IScheduleStrategy>> _incomingSchedules;
        private List<TKey> _removingSchedules;

        private Func<TKey, IScheduleStrategy> _strategyCreationFactory;
        private bool _allowMutations;
        private bool _multithreadTicks;
        private bool _cancel;
        private int _groupCounter;

        public MultiScheduler(Func<TKey, IScheduleStrategy> strategyCreationFactory)
        {
            _strategyCreationFactory = strategyCreationFactory;
            _scheduleGroups = new Dictionary<TKey, ScheduleInfo>();
            _incomingSchedules = new List<KeyValuePair<TKey, IScheduleStrategy>>();
            _removingSchedules = new List<TKey>();
            _allowMutations = true;
            _multithreadTicks = false;
            _cancel = false;
            _groupCounter = 0;
        }

        public void AddScheduleGroup(TKey scheduleKey, IScheduleStrategy strategy)
        {
            if (!_allowMutations)
            {
                _incomingSchedules.Add(new KeyValuePair<TKey, IScheduleStrategy>(scheduleKey, strategy));
            }
            else
            {
                _scheduleGroups.Add(scheduleKey, CreateScheduleInfo(strategy));
            }
        }

        public bool ContainsScheduleGroup(TKey scheduleKey)
         => _scheduleGroups.ContainsKey(scheduleKey);

        public void ChangeStrategy(TKey scheduleKey, IScheduleStrategy strategy)
        {
            var existingActions = _scheduleGroups[scheduleKey].Strategy.GetScheduledActions();

            if (!_allowMutations)
            {
                _incomingSchedules.Add(new KeyValuePair<TKey, IScheduleStrategy>(scheduleKey, strategy));
                _removingSchedules.Add(scheduleKey);
            }
            else
            {
                _scheduleGroups[scheduleKey] = CreateScheduleInfo(strategy);
            }

            foreach (var item in existingActions)
            {
                strategy.OnActionScheduled(item);
            }
        }

        public void RecreateStrategies()
        {
            bool mutationsChanged = false;

            if(_allowMutations)
            {
                mutationsChanged = true;
                _allowMutations = false;
            }

            foreach (var item in _scheduleGroups.Keys)
            {
                ChangeStrategy(item, _strategyCreationFactory(item));
            }

            ProcessMutations();
            if (mutationsChanged)
            {
                _allowMutations = true;
            }
        }

        public void RemoveStrategy(TKey scheduleKey)
        {
            if(!_allowMutations)
            {
                _removingSchedules.Add(scheduleKey);
                return;
            }
            PerformRemoval(scheduleKey);
        }

        public void Schedule(TKey scheduleKey, ActionRun action)
        {
            IScheduleStrategy? strategy = null;
            if (!_scheduleGroups.TryGetValue(scheduleKey, out var scheduler))
            {
                strategy = _strategyCreationFactory(scheduleKey);

                if (!_allowMutations)
                {
                    _incomingSchedules.Add(new KeyValuePair<TKey, IScheduleStrategy>(scheduleKey, strategy));
                }
                else
                {
                    _scheduleGroups.Add(scheduleKey, CreateScheduleInfo(strategy));
                }

            }
            else
            {
                strategy = scheduler.Strategy;
            }

            strategy.OnActionScheduled(action);
        }

        private void ScheduleNow(TKey scheduleKey, ActionRun action)
        {
            if (!_allowMutations)
            {
                throw new InvalidOperationException("You cannot schedule an action for immediate execution while the scheduler is undergoing a tick.");
            }


            if (!_scheduleGroups.TryGetValue(scheduleKey, out var scheduler))
            {
                scheduler = CreateScheduleInfo(_strategyCreationFactory(scheduleKey));
            }
            scheduler.Strategy.OnActionScheduled(action);

            if(scheduler.BackingThread != null)
            {
                scheduler.AllowTick = true;
                while (scheduler.AllowTick)
                {
                }
            }
            else
            {
                scheduler.Strategy.OnTick();
            }
        }

        public void TickAll()
        {
            _allowMutations = false;
            foreach (var schedule in _scheduleGroups)
            {
                if (_multithreadTicks)
                {
                    schedule.Value.AllowTick = true;
                }
                else
                {
                    schedule.Value.Strategy.OnTick();
                }
            }

            foreach (var schedule in _scheduleGroups)
            {
                while (schedule.Value.AllowTick)
                { 
                }
            }

            ProcessMutations();
            _allowMutations = true;
        }


        private ScheduleInfo CreateScheduleInfo(IScheduleStrategy scheduler)
        {
            var info = new ScheduleInfo(null, -1, false, scheduler);
            FillScheduleInfo(info);
            return info;
        }

        private void FillScheduleInfo(ScheduleInfo info)
        {
            _groupCounter++;
            info.InfoNumber = _groupCounter;
            if (_multithreadTicks)
            {
                var thread = new Thread(new ParameterizedThreadStart(ScheduleThread));
                thread.Name = $"Scheduler thread #{info.InfoNumber}";
                thread.IsBackground = true;

                info.BackingThread = thread;
                info.AllowTick = false;

                thread.Start(info);
            }
            else
            {
                info.BackingThread = null;
            }
        }

        private void SetupThreads()
        {
            _cancel = false;

            _allowMutations = false;
            foreach (var item in _scheduleGroups)
            {
                FillScheduleInfo(item.Value);
            }
            ProcessMutations();
            _allowMutations = true;
        }

        private void TeardownThreads()
        {
            _cancel = true;
            _allowMutations = false;
            foreach (var item in _scheduleGroups)
            {
                if(item.Value.BackingThread != null)
                {
                    item.Value.BackingThread.Join();
                    FillScheduleInfo(item.Value);
                }
            }
            ProcessMutations();
            _allowMutations = true;
        }

        private void ProcessMutations()
        {
            foreach (var item in _removingSchedules)
            {
                PerformRemoval(item);
            }
            _removingSchedules.Clear();

            foreach (var item in _incomingSchedules)
            {
                _scheduleGroups.Add(item.Key, CreateScheduleInfo(item.Value));
            }
            _incomingSchedules.Clear();
        }

        private void PerformRemoval(TKey key)
        {
            var info = _scheduleGroups[key];

            info.AllowTick = false;
            if (info.BackingThread != null)
            {
                info.CancelThread = true;
                info.BackingThread.Join();
            }

            _scheduleGroups.Remove(key);
        }

        private void ScheduleThread(object? scheduleInfo)
        {
            EngineLog.For<MultiScheduler<TKey>>().Info("Thread {threadName} scheduled for tasks.", Thread.CurrentThread.Name);
            if (scheduleInfo is ScheduleInfo info)
            {
                while (!_cancel && !info.CancelThread)
                {
                    if (info.AllowTick)
                    {
                        info.Strategy.OnTick();
                        info.AllowTick = false;
                    }
                }
            }
            EngineLog.For<MultiScheduler<TKey>>().Info("Thread {threadName} is closing and will no longer execute tasks.", Thread.CurrentThread.Name);
        }

        private class ScheduleInfo
        {
            public Thread? BackingThread;
            public bool CancelThread;
            public bool AllowTick;
            public IScheduleStrategy Strategy;
            public int InfoNumber;

            public ScheduleInfo(Thread? backingThread, int infoNumber, bool allowTick, IScheduleStrategy strategy)
            {
                BackingThread = backingThread;
                CancelThread = false;
                InfoNumber = infoNumber;
                AllowTick = allowTick;
                Strategy = strategy;
            }
        }
    }
}
