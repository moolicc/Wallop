using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Modules.SettingTypes;

namespace Wallop.Shared.Modules
{
    public record ModuleSetting(string SettingName, string SettingDescription, string? DefaultValue, string SettingType, bool Required, bool Tracked, IEnumerable<ModuleSettingBinding> Bindings, IEnumerable<KeyValuePair<string, string>> SettingTypeArgs)
    {
        public ISettingType? CachedType;
    }

    public record ModuleSettingBinding(string TypeName, string PropertyName);
}
