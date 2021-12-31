using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cog.Sources
{
    internal class TypedSettingSource : ISettingsSource
    {
        public bool CanSave => false;

        public bool HandleOrphanSettings => false;

        public bool HandleUnsavableSettings => false;

        public TypedSettingSource()
        {

        }

        public async Task<IEnumerable<KeyValuePair<string, object>>> LoadSettingsAsync()
        {
            var types = ReflectionUtils.GetSettingsTypes();

            return await Task.Run(() =>
            {
                var results = new List<KeyValuePair<string, object>>();
                foreach (var type in types)
                {
                    Settings? instance = Activator.CreateInstance(type) as Settings;
                    if (instance == null)
                    {
                        throw new InvalidOperationException("Failed to instantiate settings type.");
                    }

                    ReflectionUtils.VisitMemberValues(type, instance, (k, v) =>
                        results.Add(new KeyValuePair<string, object>(k, v)));
                }
                return results;
            });
        }

        public Task SaveSettingsAsync(IEnumerable<KeyValuePair<string, object>> settings)
        {
            throw new NotImplementedException();
        }
    }
}
