using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageGen.ChangeTracking
{
    [Flags]
    public enum ChangeTypes
    {
        Create = 2,
        Update = 4,
        Delete = 8,
        Revert = 16,

        CreateReversion = Create | Revert,
        UpdateReversion = Update | Revert,
        DeleteReversion = Delete | Revert,

    }

    public interface IChange
    {
        ChangeTypes ChangeType { get; }
        string TargetField { get; }
        string CurrentValue { get; }
        string NewValue { get; }

        IChange Revert();
    }
}
