using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.App.Layout
{
    interface ILayoutScript
    {
        void Execute(string source);
        void Cleanup();
    }
}
