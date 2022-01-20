using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.SceneManagement
{
    public class Scene
    {
        public List<Layout> Layouts { get; set; }

        public Layout? ActiveLayout { get; set; }

        public List<ECS.Director> Directors { get; set; }
        
        public Scene()
        {
            Layouts = new List<Layout> { };
            ActiveLayout = null;
            Directors = new List<ECS.Director> { };
        }
    }
}
