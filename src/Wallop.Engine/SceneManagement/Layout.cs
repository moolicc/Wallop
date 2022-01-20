using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.Types;

namespace Wallop.Engine.SceneManagement
{
    public class Layout
    {
        public ECS.Manager EcsRoot { get; set; }
        public ScreenInfo Screen { get; set; }

        public Layout()
        {
            EcsRoot = new ECS.Manager();
        }
    }
}
