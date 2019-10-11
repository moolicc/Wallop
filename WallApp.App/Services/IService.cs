using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.App.Services
{
    interface IService
    {
        int InitPriority { get; }
        void Initialize();
    }
}
