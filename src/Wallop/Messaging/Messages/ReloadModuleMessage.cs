﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Messaging.Messages
{
    public readonly record struct ReloadModuleMessage(string ModuleId, bool keepState);
}