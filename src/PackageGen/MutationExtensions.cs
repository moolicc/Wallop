using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Modules;
using Module = Wallop.Shared.Modules.Module;

namespace PackageGen
{
    public readonly record struct MutationMapping(string Key, MethodInfo Method, Delegate Target);

    public static class MutationExtensions
    {
        private static IEnumerable<(string, string)> _emptyStringTuple;
        private static IEnumerable<KeyValuePair<string, string>> _emptyStringKvp;
        private static IEnumerable<string> _emptyString;

        private static List<MutationMapping>? _mappingCache;

        static MutationExtensions()
        {
            _emptyStringTuple = Array.Empty<(string, string)>();
            _emptyStringKvp = Array.Empty<KeyValuePair<string, string>>();
            _emptyString = Array.Empty<string>();

            GetMappings();
        }

        public static MutationMapping[] GetMappings()
        {
            if(_mappingCache == null)
            {
                _mappingCache = new List<MutationMapping>();

                var myType = typeof(MutationExtensions);
                var methods = myType.GetMethods();
                foreach (var item in methods)
                {
                    var attrib = item.GetCustomAttribute<MutationMapAttribute>();
                    if (attrib != null)
                    {
                        var del = item.CreateDelegate(attrib.DelegateType);
                        var key = attrib.MapTo;

                        _mappingCache.Add(new MutationMapping(key, item, del));
                    }
                }
            }

            return _mappingCache.ToArray();
        }

        [MutationMap(nameof(Package), typeof(Func<string, Package>))]
        public static Package CreateEmptyPackage(string name)
        {
            Package package = new Package();
            package.DeclaredModules = Array.Empty<Module>();
            package.Info = new PackageInfo("", name, "", "", new List<KeyValuePair<string, string>>());
            return package;
        }

        [MutationMap(nameof(Package.Info.PackageName), typeof(Action<Package, string>))]
        public static void SetPackageName(this Package package, string name)
        {
            PackageInfo newInfo = package.Info with
            {
                PackageName = name,
            };
            package.Info = newInfo;
        }

        [MutationMap(nameof(Package.Info.PackageVersion), typeof(Action<Package, string>))]
        public static void SetPackageVersion(this Package package, string ver)
        {
            PackageInfo newInfo = package.Info with
            {
                PackageVersion = ver,
            };
            package.Info = newInfo;
        }

        [MutationMap(nameof(Package.Info.PackageDescription), typeof(Action<Package, string>))]
        public static void SetPackageDescription(this Package package, string desc)
        {
            PackageInfo newInfo = package.Info with
            {
                PackageDescription = desc,
            };
            package.Info = newInfo;
        }

        [MutationMap(nameof(Package.Info.ManifestPath), typeof(Action<Package, string>))]
        public static void SetPackagePath(this Package package, string path)
        {
            PackageInfo newInfo = package.Info with
            {
                ManifestPath = path,
            };
            package.Info = newInfo;
        }

        [MutationMap(nameof(Package.Info.PackageVariables) + ":$", typeof(Action<Package, string, string>))]
        public static void SetPackageVariable(this Package package, string key, string value)
        {
            var vars = new List<KeyValuePair<string, string>>(package.Info.PackageVariables);

            var found = false;
            for (int i = 0; i < vars.Count; i++)
            {
                if (vars[i].Key == key)
                {
                    vars[i] = new KeyValuePair<string, string>(key, value);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                vars.Add(new KeyValuePair<string, string>(key, value));
            }

            PackageInfo newInfo = package.Info with
            {
                PackageVariables = vars
            };
            package.Info = newInfo;
        }

        [MutationMap(nameof(Package.Info.PackageVariables) + ":$", typeof(Action<Package, string>))]
        public static void RemovePackageVariable(this Package package, string key)
        {
            var vars = new List<KeyValuePair<string, string>>(package.Info.PackageVariables);

            for (int i = 0; i < vars.Count; i++)
            {
                if (vars[i].Key == key)
                {
                    vars.RemoveAt(i);
                    break;
                }
            }

            PackageInfo newInfo = package.Info with
            {
                PackageVariables = vars
            };
            package.Info = newInfo;
        }

        [MutationMap(nameof(Package.DeclaredModules), typeof(Action<Package, Module>))]
        public static void AddModule(this Package package, Module module)
        {
            var mods = new List<Module>(package.DeclaredModules);
            mods.Add(module);
            package.DeclaredModules = mods.ToArray();
        }

        [MutationMap(nameof(Package.DeclaredModules) + ":$", typeof(Action<Package, string>))]
        public static void RemoveModule(this Package package, string moduleName)
        {
            var mods = new List<Module>(package.DeclaredModules);

            for (int i = 0; i < mods.Count; i++)
            {
                if (mods[i].ModuleInfo.ScriptName == moduleName)
                {
                    mods.RemoveAt(i);
                    break;
                }
            }

            package.DeclaredModules = mods.ToArray();
        }

        [MutationMap(nameof(Module), typeof(Func<Package?, string, Module>))]
        public static Module CreateEmptyModule(Package? owner, string name)
        {
            var newModule = new Module();

            newModule.ModuleInfo = new ModuleInfo("", "", "", name, "", "", "", _emptyStringKvp, ModuleTypes.Actor, _emptyString, _emptyStringKvp);
            newModule.ModuleSettings = new List<ModuleSetting>();


            if(owner != null)
            {
                AddModule(owner, newModule);
            }

            return newModule;
        }

        [MutationMap(nameof(Module.ModuleInfo.SourcePath), typeof(Action<Module, string>))]
        public static void SetModuleSourceFile(this Module module, string filepath)
        {
            module.ModuleInfo = module.ModuleInfo with
            {
                SourcePath = filepath,
            };
        }

        [MutationMap(nameof(Module.ModuleInfo.ScriptName), typeof(Action<Module, string>))]
        public static void SetModuleName(this Module module, string name)
        {
            module.ModuleInfo = module.ModuleInfo with
            {
                ScriptName = name,
            };
        }

        [MutationMap(nameof(Module.ModuleInfo.ScriptDescription), typeof(Action<Module, string>))]
        public static void SetModuleDescription(this Module module, string desc)
        {
            module.ModuleInfo = module.ModuleInfo with
            {
                ScriptDescription = desc,
            };
        }

        [MutationMap(nameof(Module.ModuleInfo.ScriptVersion), typeof(Action<Module, string>))]
        public static void SetModuleVersion(this Module module, string ver)
        {
            module.ModuleInfo = module.ModuleInfo with
            {
                ScriptVersion = ver,
            };
        }

        [MutationMap(nameof(Module.ModuleInfo.ScriptType), typeof(Action<Module, ModuleTypes>))]
        public static void SetModuleType(this Module module, ModuleTypes type)
        {
            module.ModuleInfo = module.ModuleInfo with
            {
                ScriptType = type,
            };
        }

        [MutationMap(nameof(Module.ModuleInfo.ScriptEngineId), typeof(Action<Module, string>))]
        public static void SetModuleScriptEngine(this Module module, string engine)
        {
            module.ModuleInfo = module.ModuleInfo with
            {
                ScriptEngineId = engine,
            };
        }

        [MutationMap(nameof(Module.ModuleInfo.ScriptEngineArgs) + ":$", typeof(Action<Module, string, string>))]
        public static void SetModuleScriptEngineArg(this Module module, string arg, string value)
        {
            var args = new List<KeyValuePair<string, string>>(module.ModuleInfo.ScriptEngineArgs);

            bool found = false;
            for (int i = 0; i < args.Count; i++)
            {
                if (args[i].Key == arg)
                {
                    args[i] = new KeyValuePair<string, string>(arg, value);
                    found = true;
                    break;
                }
            }

            if(!found)
            {
                args.Add(new KeyValuePair<string, string>(arg, value));
            }

            module.ModuleInfo = module.ModuleInfo with
            {
                ScriptEngineArgs = args,
            };
        }

        [MutationMap(nameof(Module.ModuleInfo.ScriptEngineArgs) + ":$", typeof(Action<Module, string>))]
        public static void RemoveModuleScriptEngineArg(this Module module, string arg)
        {
            var args = new List<KeyValuePair<string, string>>(module.ModuleInfo.ScriptEngineArgs);

            for (int i = 0; i < args.Count; i++)
            {
                if (args[i].Key == arg)
                {
                    args.RemoveAt(i);
                    break;
                }
            }

            module.ModuleInfo = module.ModuleInfo with
            {
                ScriptEngineArgs = args,
            };
        }

        [MutationMap(nameof(Module.ModuleInfo.HostApis), typeof(Action<Module, string>))]
        public static void AddModuleHostApi(this Module module, string api)
        {
            var apis = new List<string>(module.ModuleInfo.HostApis);
            apis.Add(api);
            module.ModuleInfo = module.ModuleInfo with
            {
                HostApis = apis
            };
        }

        [MutationMap(nameof(Module.ModuleInfo.HostApis) + ":$", typeof(Action<Module, string>))]
        public static void RemoveModuleHostApi(this Module module, string api)
        {
            var apis = new List<string>(module.ModuleInfo.HostApis);
            apis.Remove(api);
            module.ModuleInfo = module.ModuleInfo with
            {
                HostApis = apis
            };
        }

        [MutationMap(nameof(Module.ModuleInfo.Variables) + ":$", typeof(Action<Module, string, string>))]
        public static void SetModuleVariable(this Module module, string key, string value)
        {
            var vars = new List<KeyValuePair<string, string>>(module.ModuleInfo.Variables);

            var found = false;
            for (int i = 0; i < vars.Count; i++)
            {
                if (vars[i].Key == key)
                {
                    vars[i] = new KeyValuePair<string, string>(key, value);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                vars.Add(new KeyValuePair<string, string>(key, value));
            }

            module.ModuleInfo = module.ModuleInfo with
            {
                Variables = vars
            };
        }

        [MutationMap(nameof(Module.ModuleSettings), typeof(Action<Module, string>))]
        public static void AddModuleSetting(this Module module, string key)
        {
            var settings = new List<ModuleSetting>(module.ModuleSettings);
            settings.Add(new ModuleSetting(key, "", "", "", false, false, true, Array.Empty<ModuleSettingBinding>(), _emptyStringKvp));

            module.ModuleSettings = settings;
        }

        [MutationMap(nameof(Module.ModuleSettings) + ":$", typeof(Action<Module, string>))]
        public static void RemoveModuleSetting(this Module module, string key)
        {
            var settings = new List<ModuleSetting>(module.ModuleSettings);
            for (int i = 0; i < settings.Count; i++)
            {
                if (settings[i].SettingName == key)
                {
                    settings.RemoveAt(i);
                    break;
                }
            }

            module.ModuleSettings = settings;
        }

        [MutationMap(nameof(Module.ModuleSettings) + ":$:" + nameof(ModuleSetting.SettingName), typeof(Action<Module, string, string>))]
        public static void SetSettingName(this Module module, string key, string newName)
        {
            var settings = new List<ModuleSetting>(module.ModuleSettings);
            for (int i = 0; i < settings.Count; i++)
            {
                if (settings[i].SettingName == key)
                {
                    settings[i] = settings[i] with
                    {
                        SettingName = newName
                    };
                    break;
                }
            }

            module.ModuleSettings = settings;
        }



        [MutationMap(nameof(Module.ModuleSettings) + ":$:" + nameof(ModuleSetting.SettingDescription), typeof(Action<Module, string, string>))]
        public static void SetSettingDescription(this Module module, string key, string desc)
        {
            var settings = new List<ModuleSetting>(module.ModuleSettings);
            for (int i = 0; i < settings.Count; i++)
            {
                if (settings[i].SettingName == key)
                {
                    settings[i] = settings[i] with
                    {
                        SettingDescription = desc
                    };
                    break;
                }
            }

            module.ModuleSettings = settings;
        }

        [MutationMap(nameof(Module.ModuleSettings) + ":$:" + nameof(ModuleSetting.SettingType), typeof(Action<Module, string, string>))]
        public static void SetSettingType(this Module module, string key, string type)
        {
            var settings = new List<ModuleSetting>(module.ModuleSettings);
            for (int i = 0; i < settings.Count; i++)
            {
                if (settings[i].SettingName == key)
                {
                    settings[i] = settings[i] with
                    {
                        SettingType = type
                    };
                    break;
                }
            }

            module.ModuleSettings = settings;
        }

        [MutationMap(nameof(Module.ModuleSettings) + ":$:" + nameof(ModuleSetting.DefaultValue), typeof(Action<Module, string, string>))]
        public static void SetSettingDefault(this Module module, string key, string defaultValue)
        {
            var settings = new List<ModuleSetting>(module.ModuleSettings);
            for (int i = 0; i < settings.Count; i++)
            {
                if (settings[i].SettingName == key)
                {
                    settings[i] = settings[i] with
                    {
                        DefaultValue = defaultValue
                    };
                    break;
                }
            }

            module.ModuleSettings = settings;
        }

        [MutationMap(nameof(Module.ModuleSettings) + ":$:" + nameof(ModuleSetting.Required), typeof(Action<Module, string, bool>))]
        public static void SetSettingRequired(this Module module, string key, bool required)
        {
            var settings = new List<ModuleSetting>(module.ModuleSettings);
            for (int i = 0; i < settings.Count; i++)
            {
                if (settings[i].SettingName == key)
                {
                    settings[i] = settings[i] with
                    {
                        Required = required
                    };
                    break;
                }
            }

            module.ModuleSettings = settings;
        }

        [MutationMap(nameof(Module.ModuleSettings) + ":$:" + nameof(ModuleSetting.Tracked), typeof(Action<Module, string, bool>))]
        public static void SetSettingTracked(this Module module, string key, bool tracked)
        {
            var settings = new List<ModuleSetting>(module.ModuleSettings);
            for (int i = 0; i < settings.Count; i++)
            {
                if (settings[i].SettingName == key)
                {
                    settings[i] = settings[i] with
                    {
                        Tracked = tracked
                    };
                    break;
                }
            }

            module.ModuleSettings = settings;
        }

        [MutationMap(nameof(Module.ModuleSettings) + ":$:" + nameof(ModuleSetting.SettingTypeArgs), typeof(Action<Module, string, string, string>))]
        public static void SetSettingTypeArg(this Module module, string key, string arg, string value)
        {
            var settings = new List<ModuleSetting>(module.ModuleSettings);
            for (int i = 0; i < settings.Count; i++)
            {
                if (settings[i].SettingName == key)
                {
                    bool found = false;
                    var args = new List<KeyValuePair<string, string>>(settings[i].SettingTypeArgs);
                    foreach (var item in args)
                    {
                        if(item.Key == arg)
                        {
                            found = true;
                            args[i] = new KeyValuePair<string, string>(arg, value);
                            break;
                        }
                    }
                    if(!found)
                    {
                        args.Add(new KeyValuePair<string, string>(arg, value));
                    }

                    settings[i] = settings[i] with
                    {
                        SettingTypeArgs = args
                    };
                    break;
                }
            }

            module.ModuleSettings = settings;
        }

        [MutationMap(nameof(Module.ModuleSettings) + "$" + nameof(ModuleSetting.SettingTypeArgs), typeof(Action<Module, string, string>))]
        public static void RemoveSettingTypeArg(this Module module, string key, string arg)
        {
            var settings = new List<ModuleSetting>(module.ModuleSettings);
            for (int i = 0; i < settings.Count; i++)
            {
                if (settings[i].SettingName == key)
                {
                    var args = new List<KeyValuePair<string, string>>(settings[i].SettingTypeArgs);
                    for (int j = 0; j < args.Count; j++)
                    {
                        KeyValuePair<string, string> item = args[j];
                        if (item.Key == arg)
                        {
                            args.RemoveAt(j);
                        }
                    }

                    settings[i] = settings[i] with
                    {
                        SettingTypeArgs = args
                    };
                    break;
                }
            }

            module.ModuleSettings = settings;
        }


        [MutationMap(nameof(Module.ModuleSettings) + ":$:" + nameof(ModuleSetting.Bindings), typeof(Action<Module, string, string, string>))]
        public static void AddSettingBinding(this Module module, string key, string typeName, string propertyName)
        {
            var settings = new List<ModuleSetting>(module.ModuleSettings);
            var settingIndex = -1;
            for (int i = 0; i < settings.Count; i++)
            {
                if (settings[i].SettingName == key)
                {
                    settingIndex = i;
                    break;
                }
            }

            if(settingIndex < 0)
            {
                return;
            }

            var bindings = new List<ModuleSettingBinding>(settings[settingIndex].Bindings);

            bindings.Add(new ModuleSettingBinding(typeName, propertyName));


            settings[settingIndex] = settings[settingIndex] with
            {
                Bindings = bindings
            };
            module.ModuleSettings = settings;
        }


        [MutationMap(nameof(Module.ModuleSettings) + ":$:" + nameof(ModuleSetting.Bindings) + ":$", typeof(Action<Module, string, string>))]
        public static void RemoveSettingBinding(this Module module, string key, string propTypeName)
        {
            if(!propTypeName.Contains(','))
            {
                throw new ArgumentException("Parameter must be of the format: property,type.", nameof(propTypeName));
            }

            int commaIndex = propTypeName.IndexOf(',');
            var propName = propTypeName.Substring(0, commaIndex);
            var typeName = propTypeName.Substring(commaIndex + 1);
            RemoveSettingBinding(module, key, propName, typeName);
        }

        //[MutationMap(nameof(Module.ModuleSettings) + ":$:" + nameof(ModuleSetting.Bindings) + ":$", typeof(Action<Module, string, string, string>))]
        public static void RemoveSettingBinding(this Module module, string key, string typeName, string propertyName)
        {
            var settings = new List<ModuleSetting>(module.ModuleSettings);
            var settingIndex = -1;
            for (int i = 0; i < settings.Count; i++)
            {
                if (settings[i].SettingName == key)
                {
                    settingIndex = i;
                    break;
                }
            }

            if (settingIndex < 0)
            {
                return;
            }

            var bindings = new List<ModuleSettingBinding>(settings[settingIndex].Bindings);

            for (int i = 0; i < bindings.Count; i++)
            {
                if(bindings[i].PropertyName == propertyName && bindings[i].TypeName == typeName)
                {
                    bindings.RemoveAt(i);
                    break;
                }
            }


            settings[settingIndex] = settings[settingIndex] with
            {
                Bindings = bindings
            };
            module.ModuleSettings = settings;
        }
    }
}
