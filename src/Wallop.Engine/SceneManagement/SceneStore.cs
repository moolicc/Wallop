using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Wallop.Engine.Settings;

namespace Wallop.Engine.SceneManagement
{
    internal class SceneStore
    {
        private Dictionary<string, StoredScene> _loadedScenes;

        public SceneStore()
        {
            _loadedScenes = new Dictionary<string, StoredScene>();
        }

        public void Add(StoredScene settings)
        {
            if(!_loadedScenes.TryAdd(settings.Name, settings))
            {
                _loadedScenes[settings.Name] = settings;
            }
        }

        public void Load(string filepath)
        {
            var json = File.ReadAllText(filepath);
            var settings = JsonSerializer.Deserialize<StoredScene>(json);
            if(settings == null)
            {
                // TODO: Error
                return;
            }
            settings.ConfigFile = filepath;
            Add(settings);
        }

        public StoredScene Get(string name)
        {
            return _loadedScenes[name];
        }

        public string Save(string name)
        {
            if(!_loadedScenes.TryGetValue(name, out var settings) || settings == null)
            {
                // TODO: Error
                return "";
            }
            return JsonSerializer.Serialize(settings);
        }

        public void Remove(StoredScene scene)
        {
            _loadedScenes.Remove(scene.Name);
        }
    }
}
