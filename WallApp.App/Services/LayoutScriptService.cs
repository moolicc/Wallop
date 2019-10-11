using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.App.Services
{
    /// <summary>
    /// The <see cref="LayoutScriptService"/> executes layout scripts and acts a bridge
    /// to the actual engine.
    /// </summary>
    class LayoutScriptService : IService
    {
        public int InitPriority => 100;

        public void Initialize()
        {
        }
    }
}
