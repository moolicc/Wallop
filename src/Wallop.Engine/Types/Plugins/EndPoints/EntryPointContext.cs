﻿using PluginPantry.Extending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Types.Plugins.EndPoints
{
    public class EntryPointContext : EndPointBase, IEntryPointContext
    {
        public PluginInformation PluginInformation { get; set; }

    }
}