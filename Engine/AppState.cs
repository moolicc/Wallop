using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallApp.Services;

namespace WallApp
{
    [Service]
    class AppState
    {
        public bool IsInPreviewMode { get; private set; }
        public bool RunningIntro { get; private set; }
        public bool RunningOutro { get; private set; }

    }
}
