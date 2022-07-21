using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageGen.ChangeTracking
{
    public class ModuleChange : IChange
    {
        public ChangeTypes ChangeType { get; init; }

        public string TargetField => "module";

        public string CurrentValue { get; init; }

        public string NewValue { get; init; }

        public ModuleChange(ChangeTypes changeType, string currentValue, string newValue)
        {
            ChangeType = changeType;
            CurrentValue = currentValue;
            NewValue = newValue;
        }

        public bool IsObjectTarget(object target)
        {
            throw new NotImplementedException();
        }
    }
}
