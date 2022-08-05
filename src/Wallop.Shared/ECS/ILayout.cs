using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.ECS
{
    public interface ILayout
    {
        string Name { get; }
        bool IsActive { get; }
        Manager EntityRoot { get; set; }

        void Activate();
        void Deactivate();
    }
}
