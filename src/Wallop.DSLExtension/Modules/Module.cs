using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.DSLExtension.Modules
{
    public class Module
    {
        public ModuleInfo ModuleInfo { get; set; }
        public IEnumerable<ModuleSetting> ModuleSettings { get; set; }

    }
}
