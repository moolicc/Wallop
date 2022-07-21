using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Scheduling
{
    public interface IScheduleStrategy
    {
        public IEnumerable<ActionRun> GetScheduledActions();

        void OnActionScheduled(ActionRun action);
        void OnTick();
    }
}
