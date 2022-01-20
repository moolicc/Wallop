﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.DSLExtension.Modules
{
    public record ModuleSetting(string SettingName, string SettingDescription, string DefaultValue, object SettingType, bool Required);
}
