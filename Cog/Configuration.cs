using Cog.Sources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cog
{
    public class Configuration
    {
        public ConfigurationOptions Options { get; set; }

        private ConcurrentDictionary<string, Settings> _bindingInstances;
        private ConcurrentDictionary<string, SettingInfo> _settingValues;

        public Configuration()
        {
            Options = new ConfigurationOptions();

            _bindingInstances = new ConcurrentDictionary<string, Settings>();
            _settingValues = new ConcurrentDictionary<string, SettingInfo>();
        }

        public async Task LoadSettingsAsync()
        {
            foreach (var source in Options.Sources)
            {
                foreach (var entry in await source.LoadSettingsAsync())
                {
                    if (_settingValues.ContainsKey(entry.Key))
                    {
                        _settingValues[entry.Key].SetValue(entry.Value);
                    }
                    else
                    {
                        _settingValues.WaitAdd(entry.Key, new SettingInfo(source, entry.Value));
                    }
                }
            }

            if(Options.ConfigureBindings)
            {
                await ResolveBindingsAsync();
            }
        }

        public async Task ResolveBindingsAsync()
        {
            var types = ReflectionUtils.GetSettingsTypes();
            foreach (var type in types)
            {
                await ResolveBindingsAsync(type);
            }
        }
        
        public async Task<T?> ResolveBindingsAsync<T>()
        {
            return (T?)await ResolveBindingsAsync(typeof(T));
        }

        public async Task<object?> ResolveBindingsAsync(Type type)
        {
            object? resultingInstance = Activator.CreateInstance(type);
            if(resultingInstance == null)
            {
                return null;
            }

            var memberNames = ReflectionUtils.GetMemberNames(type);
            await Task.Run(() =>
            {
                foreach (var item in _settingValues)
                {
                    ReflectionUtils.VisitMembers(type, resultingInstance, (p, f) =>
                    {
                        if(p != null && p.Name == item.Key)
                        {
                            item.Value.Binding = new SettingBinding(resultingInstance, p);
                            _bindingInstances.WaitAdd(p.Name, (Settings)resultingInstance);
                            return true;
                        }
                        else if (f != null && f.Name == item.Key)
                        {
                            item.Value.Binding = new SettingBinding(resultingInstance, f);
                            _bindingInstances.WaitAdd(f.Name, (Settings)resultingInstance);
                            return true;
                        }
                        return false;
                    });
                }
            });
            return resultingInstance;
        }

        public void Set(string key, object value)
        {
            _settingValues.GetOrAdd(key, new SettingInfo(null, value)).SetValue(value);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            return await Task.Run(() =>
                 Get<T>(key));
        }

        public T Get<T>(string key)
        {
            if (typeof(T).Inherits<Settings>())
            {
                if (_bindingInstances.TryGetValue(key, out var value))
                {
                    return (T)(object)value;
                }
            }
            return _settingValues[key].GetValue<T>();
        }

        public async Task<T> GetAsync<T>()
        {
            return await Task.Run(() =>
            {
                if (typeof(T).Inherits<Settings>())
                {
                    Settings? settingsInstance = null;
                    foreach (var item in _bindingInstances)
                    {
                        if (item.Value.GetType() == typeof(T))
                        {
                            settingsInstance = item.Value;
                            break;
                        }
                    }
                    if (settingsInstance == null)
                    {
                        throw new KeyNotFoundException();
                    }
                    return (T)(object)settingsInstance;
                }
                foreach (var item in _settingValues)
                {
                    if (item.Value.ValueType == typeof(T))
                    {
                        return (T)item.Value.GetValue();
                    }
                }
                throw new KeyNotFoundException();
            });
        }

        public T Get<T>()
        {
            var get = GetAsync<T>();
            return get.Result;
        }

        public void Persist()
        {
            var orphanSources = Options.Sources.Where(s => s.HandleOrphanSettings);
            var fosterSources = Options.Sources.Where(s => s.HandleUnsavableSettings);
            var aggregatedSources = new Dictionary<ISettingsSource, List<KeyValuePair<string, SettingInfo>>>(Options.Sources.Count);

            foreach (var item in _settingValues)
            {
                var source = item.Value.Source;
                if(source == null)
                {
                    foreach (var orphanHandler in orphanSources)
                    {
                        AddToAggregation(ref aggregatedSources, orphanHandler, item);
                    }
                }
                else if (!source.CanSave)
                {
                    foreach (var unsavableHandler in fosterSources)
                    {
                        AddToAggregation(ref aggregatedSources, unsavableHandler, item);
                    }
                }
                else
                {
                    AddToAggregation(ref aggregatedSources, source, item);
                }
            }

            PersistFromAggregation(aggregatedSources);
        }

        public void Persist(params string[] keys)
        {
            var orphanSources = Options.Sources.Where(s => s.HandleOrphanSettings);
            var fosterSources = Options.Sources.Where(s => s.HandleUnsavableSettings);
            var aggregatedSources = new Dictionary<ISettingsSource, List<KeyValuePair<string, SettingInfo>>>(Options.Sources.Count);

            foreach (var item in _settingValues)
            {
                if(!keys.Contains(item.Key))
                {
                    continue;
                }

                var source = item.Value.Source;
                if (source == null)
                {
                    foreach (var orphanHandler in orphanSources)
                    {
                        AddToAggregation(ref aggregatedSources, orphanHandler, item);
                    }
                }
                else if(!source.CanSave)
                {
                    foreach (var unsavableHandler in fosterSources)
                    {
                        AddToAggregation(ref aggregatedSources, unsavableHandler, item);
                    }
                }
                else
                {
                    AddToAggregation(ref aggregatedSources, source, item);
                }
            }

            PersistFromAggregation(aggregatedSources);
        }


        /* TODO: Add this when I need it.
        public void Persist<T>()
        {

            if (typeof(T).Inherits<Settings>())
            {
                Settings? settingsInstance = null;
                foreach (var item in _bindingInstances)
                {
                    if(item.Value.GetType() == typeof(T))
                    {
                        settingsInstance = item.Value;
                        break;
                    }
                }
                if(settingsInstance == null)
                {
                    return;
                }
                PersistSettings(settingsInstance);
            }
            else
            {
                var key = _settingValues.FirstOrDefault(kvp => kvp.Value.GetValue().GetType() == typeof(T));
            }

            var orphanSources = Options.Sources.Where(s => s.HandleOrphanSettings);
            var aggregatedSources = new Dictionary<ISettingsSource, List<KeyValuePair<string, SettingInfo>>>(Options.Sources.Count);
        }
        
        private void PersistSettings(Settings instance)
        {
            var orphanSources = Options.Sources.Where(s => s.HandleOrphanSettings);
            var aggregatedSources = new Dictionary<ISettingsSource, List<KeyValuePair<string, SettingInfo>>>(Options.Sources.Count);

            foreach (var item in _settingValues)
            {
                if (item.Value.Binding == null)
                {
                    continue;
                }
                if (item.Value.Binding.Instance == instance)
                {
                    var source = item.Value.Source;
                    if (source == null)
                    {
                        foreach (var orphanHandler in orphanSources)
                        {
                            AddToAggregation(ref aggregatedSources, orphanHandler, item);
                        }
                    }
                    else
                    {
                        AddToAggregation(ref aggregatedSources, source, item);
                    }
                }
            }

            PersistFromAggregation(aggregatedSources);
        }
        */

        private void AddToAggregation(ref Dictionary<ISettingsSource, List<KeyValuePair<string, SettingInfo>>> aggregation, ISettingsSource source, KeyValuePair<string, SettingInfo> value)
        {
            aggregation.TryAdd(source, new List<KeyValuePair<string, SettingInfo>>());
            aggregation[source].Add(value);
        }

        private void PersistFromAggregation(Dictionary<ISettingsSource, List<KeyValuePair<string, SettingInfo>>> aggregations)
        {
            var tasks = new Task[aggregations.Count];
            var i = 0;

            foreach (var item in aggregations)
            {
                tasks[i] = item.Key.SaveSettingsAsync(item.Value.ToValues());
                i++;
            }

            Task.WaitAll(tasks);
        }
    }
}
