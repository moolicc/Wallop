using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cog.Sources
{
    public class TypedSettingSource : ISettingsSource
    {
        public bool CanSave => false;

        public bool HandleOrphanSettings => false;

        public bool HandleUnsavableSettings => false;

        public TypedSettingSource()
        {

        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> LoadSettingsAsync(ConfigurationOptions options)
        {
            var types = ReflectionUtils.GetSettingsTypes();

            return await Task.Run(() =>
            {
                var results = new List<KeyValuePair<string, string>>();
                foreach (var type in types)
                {
                    Settings? instance = Activator.CreateInstance(type) as Settings;
                    if (instance == null)
                    {
                        throw new InvalidOperationException("Failed to instantiate settings type.");
                    }

                    ReflectionUtils.VisitMemberValues(type, instance, (k, v) =>
                        results.Add(new KeyValuePair<string, string>(options.FlattenTree ? k : $"{type.Name}{options.HierarchyDelimiter}{k}", Serializer.Serialize(v.GetType(), v))));
                }
                return results;
            });
        }

        public Task SaveSettingsAsync(IEnumerable<KeyValuePair<string, string>> settings, ConfigurationOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
