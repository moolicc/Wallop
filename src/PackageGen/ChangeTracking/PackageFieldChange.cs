using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageGen.ChangeTracking
{
    public class PackageFieldChange : IChange
    {
        public ChangeTypes ChangeType { get; init; }

        public string TargetField { get; init; }

        public string CurrentValue { get; init; }

        public string NewValue { get; init; }

        public PackageFieldChange(ChangeTypes changeType, string targetField, string currentValue, string newValue)
        {
            ChangeType = changeType;
            TargetField = targetField;
            CurrentValue = currentValue;
            NewValue = newValue;
        }


        public bool IsObjectTarget(object target)
        {
            throw new NotImplementedException();
        }
    }
}
