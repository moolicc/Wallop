using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.DSLExtension.Modules
{
    public record ModuleSetting(string SettingName, string SettingDescription, string? DefaultValue, string SettingType, bool Required, IEnumerable<ModuleSettingBinding> Bindings, IEnumerable<KeyValuePair<string, string>> SettingTypeArgs)
    {
        public SettingTypes.ISettingType? CachedType;
    }

    public record ModuleSettingBinding(string TypeName, string PropertyName);
}
