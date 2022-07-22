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

        public IChange Revert()
        {
            switch (ChangeType)
            {
                case ChangeTypes.Create:
                    break;
                case ChangeTypes.Update:
                    return new ModuleChange(ChangeTypes.UpdateReversion, NewValue, CurrentValue);
                case ChangeTypes.Delete:
                    break;
                default:
                    break;
            }

            throw new NotImplementedException();
        }
    }
}
