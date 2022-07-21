using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Modules;

namespace PackageGen
{
    public static class MutationExtensions
    {
        private static IEnumerable<(string, string)> _emptyStringTuple;
        private static IEnumerable<KeyValuePair<string, string>> _emptyStringKvp;
        private static IEnumerable<string> _emptyString;

        static MutationExtensions()
        {
            _emptyStringTuple = Array.Empty<(string, string)>();
            _emptyStringKvp = Array.Empty<KeyValuePair<string, string>>();
            _emptyString = Array.Empty<string>();
        }

        public static Package CreateEmptyPackage(string name)
        {
            Package package = new Package();
            package.DeclaredModules = Array.Empty<Module>();
            package.Info = new PackageInfo("", name, "", "", new List<(string, string)>());
            return package;
        }

        public static void SetPackageName(this Package package, string name)
        {
            PackageInfo newInfo = package.Info with
            {
                PackageName = name,
            };
            package.Info = newInfo;
        }

        public static void SetPackageVersion(this Package package, string ver)
        {
            PackageInfo newInfo = package.Info with
            {
                PackageVersion = ver,
            };
            package.Info = newInfo;
        }

        public static void SetPackageDescription(this Package package, string desc)
        {
            PackageInfo newInfo = package.Info with
            {
                PackageDescription = desc,
            };
            package.Info = newInfo;
        }

        public static void SetPackagePath(this Package package, string path)
        {
            PackageInfo newInfo = package.Info with
            {
                ManifestPath = path,
            };
            package.Info = newInfo;
        }

        public static void SetPackageVariable(this Package package, string key, string value)
        {
            var vars = new List<(string, string)>(package.Info.PackageVariables);

            var found = false;
            for (int i = 0; i < vars.Count; i++)
            {
                if (vars[i].Item1 == key)
                {
                    vars[i] = (key, value);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                vars.Add((key, value));
            }

            PackageInfo newInfo = package.Info with
            {
                PackageVariables = vars
            };
            package.Info = newInfo;
        }

        public static void RemovePackageVariable(this Package package, string key)
        {
            var vars = new List<(string, string)>(package.Info.PackageVariables);

            for (int i = 0; i < vars.Count; i++)
            {
                if (vars[i].Item1 == key)
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

        public static void AddModule(this Package package, Module module)
        {
            var mods = new List<Module>(package.DeclaredModules);
            mods.Add(module);
            package.DeclaredModules = mods.ToArray();
        }

        public static Module CreateEmptyModule(Package? owner, string name)
        {
            var newModule = new Module();

            newModule.ModuleInfo = new ModuleInfo("", "", "", name, "", "", "", _emptyStringKvp, ModuleTypes.Actor, _emptyString, _emptyStringTuple);
            newModule.ModuleSettings = new List<ModuleSetting>();


            if(owner != null)
            {
                AddModule(owner, newModule);
            }

            return newModule;
        }

        public static void SetModuleSourceFile(this Module module, string filepath)
        {
            module.ModuleInfo = module.ModuleInfo with
            {
                SourcePath = filepath,
            };
        }

        public static void SetModuleName(this Module module, string name)
        {
            module.ModuleInfo = module.ModuleInfo with
            {
                ScriptName = name,
            };
        }



        public static void SetModuleDescription(this Module module, string desc)
        {
            module.ModuleInfo = module.ModuleInfo with
            {
                ScriptDescription = desc,
            };
        }

        public static void SetModuleVersion(this Module module, string ver)
        {
            module.ModuleInfo = module.ModuleInfo with
            {
                ScriptVersion = ver,
            };
        }

        public static void SetModuleType(this Module module, ModuleTypes type)
        {
            module.ModuleInfo = module.ModuleInfo with
            {
                ScriptType = type,
            };
        }

        public static void SetModuleScriptEngine(this Module module, string engine)
        {
            module.ModuleInfo = module.ModuleInfo with
            {
                ScriptEngineId = engine,
            };
        }

        public static void AddModuleHostApi(this Module module, string api)
        {
            var apis = new List<string>(module.ModuleInfo.HostApis);
            apis.Add(api);
            module.ModuleInfo = module.ModuleInfo with
            {
                HostApis = apis
            };
        }

        public static void RemoveModuleHostApi(this Module module, string api)
        {
            var apis = new List<string>(module.ModuleInfo.HostApis);
            apis.Remove(api);
            module.ModuleInfo = module.ModuleInfo with
            {
                HostApis = apis
            };
        }

        public static void SetModuleVariable(this Module module, string key, string value)
        {
            var vars = new List<(string, string)>(module.ModuleInfo.Variables);

            var found = false;
            for (int i = 0; i < vars.Count; i++)
            {
                if (vars[i].Item1 == key)
                {
                    vars[i] = (key, value);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                vars.Add((key, value));
            }

            module.ModuleInfo = module.ModuleInfo with
            {
                Variables = vars
            };
        }

        public static void AddModuleSetting(this Module module, string key)
        {
            var settings = new List<ModuleSetting>(module.ModuleSettings);
            settings.Add(new ModuleSetting(key, "", null, "", false, false, Array.Empty<ModuleSettingBinding>(), _emptyStringKvp));

            module.ModuleSettings = settings;
        }

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
