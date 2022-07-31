using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Settings
{
    public class DependencyDirectory
    {
        public string Directory { get; set; } = string.Empty;
        public bool Recursive { get; set; } = false;
    }
}
