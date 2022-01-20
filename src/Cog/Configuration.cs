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

        private ConcurrentDictionary<string, object> _bindingInstances;
        private ConcurrentDictionary<string, SettingInfo> _settingValues;

        public Configuration()
        {
            Options = new ConfigurationOptions();

            _bindingInstances = new ConcurrentDictionary<string, object>();
            _settingValues = new ConcurrentDictionary<string, SettingInfo>();
        }

        public async Task LoadSettingsAsync()
        {
            foreach (var source in Options.Sources)
            {
                foreach (var entry in await source.LoadSettingsAsync(Options))
                {
                    if (_settingValues.ContainsKey(entry.Key))
                    {
                        _settingValues[entry.Key].SetLiteralValue(entry.Value);
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
            //TODO: What was this for?
            //LoadIndependentSettingInstances();
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
            var instanceName = resultingInstance.GetType().Name;
            if(resultingInstance.GetType().Inherits<Settings>())
            {
                var settingsInstance = resultingInstance as Settings;
                if(settingsInstance != null)
                {
                    instanceName = settingsInstance.GetHierarchyName();
                }
            }


            var memberNames = ReflectionUtils.GetMemberNames(type);
            await Task.Run(() =>
            {
                foreach (var item in _settingValues)
                {
                    ReflectionUtils.VisitMembers(type, resultingInstance, (p, f) =>
                    {
                        if(p != null)
                        {
                            var hierarchyKey = string.Format("{0}{1}{2}", instanceName, Options.HierarchyDelimiter, p.Name);
                            var flatKey = p.Name;
                            var settingName = Options.FlattenTree ? flatKey : hierarchyKey;


                            if (item.Key == hierarchyKey
                            || (item.Key == flatKey && Options.FlattenTree))
                            {
                                item.Value.Binding = new SettingBinding(resultingInstance, p);
                                _bindingInstances.WaitAdd(settingName, (Settings)resultingInstance);
                                item.Value.SetValue(item.Value.GetValue());
                                return true;
                            }

                        }
                        else if (f != null)
                        {
                            var hierarchyKey = string.Format("{0}{1}{2}", instanceName, Options.HierarchyDelimiter, f.Name);
                            var flatKey = f.Name;
                            var settingName = Options.FlattenTree ? flatKey : hierarchyKey;

                            if (item.Key == hierarchyKey
                            || (item.Key == flatKey && Options.FlattenTree))
                            {
                                item.Value.Binding = new SettingBinding(resultingInstance, f);
                                _bindingInstances.WaitAdd(settingName, (Settings)resultingInstance);
                                item.Value.SetValue(item.Value.GetValue());
                                return true;
                            }
                        }
                        return false;
                    });
                }
            });
            return resultingInstance;
        }

        private void LoadIndependentSettingInstances()
        {
            // TODO: Make this operation a flag in the options?
            foreach (var type in ReflectionUtils.GetSettingsTypes())
            {
                if(_bindingInstances.Values.Any(x => x.GetType() == type))
                {
                    continue;
                }
                var instance = Activator.CreateInstance(type);
                if(instance == null)
                {
                    throw new NullReferenceException();
                }

                ReflectionUtils.VisitMembers(type, instance, (p, f) =>
                {
                    var name = p?.Name ?? f?.Name;
                    var flatName = name;

                    if (p == null && f == null)
                    {
                        throw new NullReferenceException();
                    }
                    object? value = null;
                    SettingInfo? settingInfo = null;

                    if (p == null)
                    {
                        value = f.GetValue(instance);
                        settingInfo = new SettingInfo(value);
                        settingInfo.Binding = new SettingBinding(instance, f);
                    }
                    else
                    {
                        value = p.GetValue(instance);
                        settingInfo = new SettingInfo(value);
                        settingInfo.Binding = new SettingBinding(instance, p);
                    }

                    if(!Options.FlattenTree)
                    {
                        name = string.Format("{0}{1}{2}", type.Name, Options.HierarchyDelimiter, name);
                    }

                    _settingValues.WaitAdd(name, settingInfo);
                    _bindingInstances.WaitAdd(name, (Settings)instance);
                });
            }
        }

        public void Set(string key, object value)
        {
            _settingValues.GetOrAdd(key, new SettingInfo(value)).SetValue(value);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            return await Task.Run(() =>
                 Get<T>(key));
        }

        public T? Get<T>(string key)
        {
            if (typeof(T).Inherits<Settings>())
            {
                // TODO: This won't work right.
                if (_bindingInstances.TryGetValue(key, out var value))
                {
                    return (T)(object)value;
                }
                else
                {
                    // Auto create an instance of T if there are members in the values dictionary
                    // with names that look like the belong to T. (ie "{typeof(T).Name()}.*")
                }
                throw new KeyNotFoundException();
            }
            return _settingValues[key].GetValue<T>();
        }

        public async Task<T?> GetAsync<T>()
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
                            settingsInstance = (Settings)item.Value;
                            break;
                        }
                    }
                    if (settingsInstance == null)
                    {
                        //throw new KeyNotFoundException();
                        return default(T);
                    }
                    return (T)(object)settingsInstance;
                }
                foreach (var item in _settingValues)
                {
                    if (item.Value.ValueType == typeof(T))
                    {
                        T? value = item.Value.GetValue<T>();
                        return value;
                    }
                }
                throw new KeyNotFoundException();
            });
        }

        public T? Get<T>()
        {
            var get = GetAsync<T>();
            return get.Result;
        }

        public IEnumerable<KeyValuePair<string, object?>> GetValues(bool preserveHierarchy = true)
        {
            if(preserveHierarchy)
            {
                foreach (var setting in _bindingInstances.DistinctBy(kvp => kvp.Value))
                {
                    yield return new KeyValuePair<string, object?>(setting.Value.GetType().Name, setting.Value);
                }
            }

            foreach (var item in _settingValues)
            {
                if(preserveHierarchy && item.Value.Binding != null)
                {
                    continue;
                }
                yield return new KeyValuePair<string, object?>(item.Key, item.Value.GetValue());
            }
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
                var dictionary = new Dictionary<string, string>();
                foreach (var setting in item.Value)
                {
                    dictionary.Add(setting.Key, setting.Value.ValueLiteral);
                }
                tasks[i] = item.Key.SaveSettingsAsync(dictionary, Options);
                i++;
            }

            Task.WaitAll(tasks);
        }
    }
}
