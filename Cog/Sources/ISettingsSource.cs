using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cog.Sources
{
    public interface ISettingsSource
    {
        bool CanSave { get; }

        bool HandleOrphanSettings { get; }
        bool HandleUnsavableSettings { get; }

        Task<IEnumerable<KeyValuePair<string, string>>> LoadSettingsAsync(ConfigurationOptions options);
        Task SaveSettingsAsync(IEnumerable<KeyValuePair<string, string>> settings, ConfigurationOptions options);
    }
}
