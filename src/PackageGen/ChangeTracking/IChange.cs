using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageGen.ChangeTracking
{
    public enum ChangeTypes
    {
        Create,
        Update,
        Delete
    }

    public interface IChange
    {
        ChangeTypes ChangeType { get; }
        string TargetField { get; }
        string CurrentValue { get; }
        string NewValue { get; }

        bool IsObjectTarget(object target);
    }
}
