﻿using PluginPantry;
using Wallop.Shared.Scripting;
using Wallop.Shared.Types.Plugin;

namespace HostApis
{
    public class Plugin
    {
        [EntryPoint("Host APIs", "1.0.0.0")]
        public void Startup(PluginContext context, Guid id)
        {
            context.RegisterExtension<IHostApi, TrippyGLApi>(id);
            context.RegisterExtension<IHostApi, EasyRenderApi>(id);
        }
    }
}