using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.App.Modules
{
    class Module
    {
        //Should we also have a Module.UID - unique id?

        public string Name { get; private set; }
        public string Description { get; private set; }
        public Version Version { get; private set; }
        public string Directory { get; private set; }
        public string WindowXaml { get; private set; }
        public string WindowScript { get; private set; }
        public string LayerScript { get; private set; }
    }
}
