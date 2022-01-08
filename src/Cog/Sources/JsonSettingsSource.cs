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

        public async Task<IEnumerable<KeyValuePair<string, string>>> LoadSettingsAsync(ConfigurationOptions options)
        {
            void VisitJsonElement(JsonProperty element, string hierarchy, Dictionary<string, string> contents)
            {
                var tree = $"{hierarchy}{options.HierarchyDelimiter}{element.Name}".Trim(options.HierarchyDelimiter.ToCharArray());
                if(options.FlattenTree)
                {
                    tree = element.Name;
                }

                if (element.Value.ValueKind == JsonValueKind.Object)
                {
                    foreach (var item in element.Value.EnumerateObject())
                    {
                        VisitJsonElement(item, tree, contents);
                    }
                }
                else
                {
                    contents.Add(tree, element.Value.ToString());
                }
            }

            if(!File.Exists(JsonFile))
            {
                return new Dictionary<string, string>();
            }
            
            return await Task.Run(() =>
            {
                var root = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(JsonFile));
                var contents = new Dictionary<string, string>(); //JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(JsonFile));

                foreach(var subNode in root.EnumerateObject())
                {
                    VisitJsonElement(subNode, "", contents);
                }
                
                if (contents == null)
                {
                    throw new FileLoadException("Failed to load settings file.");
                }

                return contents;
            });
        }

        public async Task SaveSettingsAsync(IEnumerable<KeyValuePair<string, string>> settings, ConfigurationOptions options)
        {
            var currentSettings = new Dictionary<string, string>(await LoadSettingsAsync(options));
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
