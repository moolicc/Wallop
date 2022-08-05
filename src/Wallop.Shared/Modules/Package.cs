using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Modules
{
    public class Package
    {
        public PackageInfo Info { get; set; }

        public Module[] DeclaredModules { get; set; }
    }
}
