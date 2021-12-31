using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cog.Sources
{
    public class JsonSettingsSource : ISettingsSource
    {
        public string JsonFile { get; set; }

        public bool CanSave => true;

        public bool HandleOrphanSettings { get; set; }

        public bool HandleUnsavableSettings { get; set; }

        public JsonSettingsSource(string jsonFile)
        {
            JsonFile = jsonFile;
            HandleOrphanSettings = true;
            HandleUnsavableSettings = true;
        }

        public async Task<IEnumerable<KeyValuePair<string, object>>> LoadSettingsAsync()
        {
            if(!File.Exists(JsonFile))
            {
                return new Dictionary<string, object>();
            }

            return await Task.Run(() =>
            {
                var contents = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(JsonFile));

                if (contents == null)
                {
                    throw new FileLoadException("Failed to load settings file.");
                }

                return contents;
            });
        }

        public async Task SaveSettingsAsync(IEnumerable<KeyValuePair<string, object>> settings)
        {
            var currentSettings = new Dictionary<string, object>(await LoadSettingsAsync());
            foreach (var item in settings)
            {
                if(!currentSettings.TryAdd(item.Key, item.Value))
                {
                    currentSettings[item.Key] = item.Value;
                }
            }

            using (var writer = new FileStream(JsonFile, FileMode.Create))
            {
                await JsonSerializer.SerializeAsync(writer, currentSettings);
            }
        }
    }
}
