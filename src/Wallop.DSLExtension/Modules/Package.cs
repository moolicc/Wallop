﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.DSLExtension.Modules
{
    public class Package
    {
        public PackageInfo Info { get; set; }

        public IEnumerable<Module> DeclaredModules { get; set; }
    }
}
