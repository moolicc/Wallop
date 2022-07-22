
using PackageGen;
using PackageGen.ChangeTracking;
using System.CommandLine;
using Wallop.DSLExtension.Modules;

namespace PackageGen

{
    static class Program
    {
        private static List<Package> _packages = new List<Package>();
        private static Package? _selectedPackage;
        private static Module? _selectedModule;
        private static bool _repl = true;
        private static ChangeSet _changes;

        private static ConsoleHelper _helper;

        static int Main(string[] args)
        {
            _packages = PackageLoader.LoadPackages(Environment.CurrentDirectory).ToList();

            if (!_packages.Any())
            {
                Console.WriteLine("No packages found in current directory. Exiting");
                return 0;
            }


            _changes = new ChangeSet();
            var command = BuildCommandTree();

            _helper = new ConsoleHelper();
            SetPrompt();
            AddKeywords();

            foreach (var item in command)
            {
                if(item is Command c)
                {
                    AddCompletions(c, _helper.Completions);
                }
            }

            command.Invoke("--help");
            while (_repl)
            {
                var input = _helper.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                command.Invoke(input);
                Console.WriteLine();
            }


            return 0;
        }

       
        private static RootCommand BuildCommandTree()
        {

            var listPkgCommand = new Command("packages", "Lists all loaded packages");
            listPkgCommand.SetHandler(_ =>
            {
                for (int i = 0; i < _packages.Count; i++)
                {
                    Console.WriteLine("{0}: {1}{2}", i, _packages[i].Info.PackageName, _packages[i] == _selectedPackage ? "*" : "");
                }
                Console.WriteLine("{0}: <New Package>", _packages.Count);

                Console.WriteLine("Run 'use package (#/[name])' to select an active package.");
            });



            var allModsOption = new Option<bool>(new[] { "--all", "-a" }, _ => false, description: "Optionally show all modules (also those not in the currently selected package).");

            var listModCommand = new Command("modules", "Lists loaded modules.")
            {
                allModsOption
            };
            listModCommand.SetHandler(a =>
            {
                if (_selectedPackage == null || a)
                {
                    int moduleCount = 0;
                    foreach (var pkg in _packages)
                    {
                        Console.WriteLine($"Modules from {pkg.Info.PackageName}");
                        foreach (var module in pkg.DeclaredModules)
                        {
                            Console.WriteLine("{0}: {1}{2}", moduleCount, module.ModuleInfo.ScriptName, module == _selectedModule ? "*" : "");
                            moduleCount++;
                        }
                        Console.WriteLine();
                    }
                }
                else
                {
                    var modules = _selectedPackage.DeclaredModules;
                    for (int i = 0; i < modules.Length; i++)
                    {
                        Console.WriteLine("{0}: {1}{2}", i, modules[i].ModuleInfo.ScriptName, modules[i] == _selectedModule ? "*" : "");
                    }
                }

                Console.WriteLine("Run 'use module (#/[name])' to select an active module.");

            }, allModsOption);


            var listCommand = new Command("list", "Performs listing operations.")
            {
                listPkgCommand,
                listModCommand
            };



            var packageArg = new Argument<string>("number/name", "The package number or name. If a name is specified and a package does not exist by that name, creates a package using that name.");
            var usePkgCommand = new Command("package", "Performs the operation on a package.")
            {
                packageArg,
            };
            usePkgCommand.SetHandler(s =>
            {
                if(int.TryParse(s, out var i))
                {
                    if (i < 0 || i > _packages.Count)
                    {
                        Console.WriteLine("Invalid package number.");
                        return;
                    }
                    _selectedPackage = _packages[i];
                    Console.WriteLine($"Using package '{_selectedPackage.Info.PackageName}'.");
                    _selectedModule = null;
                }
                else
                {
                    _selectedPackage = _packages.FirstOrDefault(p => p.Info.PackageName == s);
                    if (_selectedPackage == null)
                    {
                        // Add new package to changelist.
                        Console.WriteLine($"New package '{s}' added to changeset.");
                        _selectedPackage = MutationExtensions.CreateEmptyPackage(s);
                        _packages.Add(_selectedPackage);

                        AddKeyword(s);
                    }
                }

                SetPrompt();
            }, packageArg);

            var moduleArg = new Argument<string>("number/name", "The module number or name. If a name is specified and a module does not exist by that name, creates a module using that name."); ;
            var useModuleCommand = new Command("module", "Performs the operation on a module.")
            {
                moduleArg,
            };
            useModuleCommand.SetHandler(s =>
            {
                if (int.TryParse(s, out var i))
                {
                    if (i < 0 || i > _packages.Count)
                    {
                        Console.WriteLine("Invalid module number.");
                        return;
                    }

                    if(_selectedPackage != null)
                    {
                        _selectedModule = _selectedPackage.DeclaredModules.ElementAt(i);
                    }
                    else
                    {
                        int moduleCount = 0;
                        foreach (var pkg in _packages)
                        {
                            bool found = false;
                            foreach (var module in pkg.DeclaredModules)
                            {
                                if(moduleCount == i)
                                {
                                    _selectedPackage = pkg;
                                    Console.WriteLine($"Using package '{pkg.Info.PackageName}'.");

                                    _selectedModule = module;
                                    found = true;
                                    break;
                                }
                                moduleCount++;
                            }

                            if(found)
                            {
                                break;
                            }
                        }
                    }
                    if(_selectedModule == null)
                    {
                        Console.WriteLine($"Module not found.");
                        return;
                    }

                    Console.WriteLine($"Using module '{_selectedModule.ModuleInfo.ScriptName}'.");
                }
                else
                {
                    _selectedModule = _selectedPackage?.DeclaredModules.FirstOrDefault(m => m.ModuleInfo.ScriptName == s);
                    if (_selectedModule == null)
                    {

                        _selectedModule = MutationExtensions.CreateEmptyModule(_selectedPackage, s);
                        _changes.Changes.Enqueue(new ModuleChange(ChangeTypes.Create, "", s));

                        Console.WriteLine($"New module '{s}' added to changeset.");
                        AddKeyword(s);
                    }
                }

                SetPrompt();
            }, moduleArg);

            var useCommand = new Command("use", "Selects or creates the active package or module.")
            {
                usePkgCommand,
                useModuleCommand
            };


            var exitCommand = new Command("exit", "Exists the program.");
            exitCommand.SetHandler(_ => _repl = false);


            var infoCommand = new Command("tree", "Shows a treeview of the currently loaded packages.");
            infoCommand.SetHandler(_ =>
            {
                foreach (var item in _packages)
                {
                    Console.WriteLine(item.ToTreeString(_changes));
                }
            });

            var changesCommand = new Command("changes", "Lists the current changes that are pending.");
            changesCommand.SetHandler(_ =>
            {
                _changes.PrintChangeSet();
            });

            var root = new RootCommand("Package generator and editor.")
            {
                infoCommand,
                listCommand,
                useCommand,
                GetSetCommand(),
                GetAddCommand(),
                GetRemoveCommand(),
                changesCommand,
                exitCommand,
            };

            return root;
        }

        private static Command GetSetCommand()
        {
            var setCommand = new Command("set", "Performs value setting operations.")
            {
                GetSetPackageCommand(),
                GetSetModuleCommand()
            };


            return setCommand;
        }

        private static Command GetAddCommand()
        {
            var addCommand = new Command("add", "Performs value creating operations.")
            {
                GetAddModuleCommand(),
            };


            return addCommand;
        }

        private static Command GetRemoveCommand()
        {
            var rmCommand = new Command("remove", "Performs value removal operations.")
            {
                GetRemovePackageCommand(),
                GetRemoveModuleCommand()
            };


            return rmCommand;
        }

        private static Command GetSetPackageCommand()
        {
            var nameArg = new Argument<string>("value", "The new name.");
            var setNameCommand = new Command("name", "Sets the name of the package.")
            {
                nameArg
            };
            setNameCommand.SetHandler(s =>
            {
                if (!CheckActivePackage() || _selectedPackage == null)
                {
                    return;
                }
                _changes.Changes.Enqueue(new PackageChange(ChangeTypes.Update, _selectedPackage.Info.PackageName, s));
                _selectedPackage.SetPackageName(s);
                Console.WriteLine($"Package name update added to changeset.");
            }, nameArg);


            var verArg = new Argument<string>("ver", "The new version.");
            var setVerCommand = new Command("version", "Sets the version of the package.")
            {
                verArg
            };
            setVerCommand.SetHandler(s =>
            {
                if (!CheckActivePackage() || _selectedPackage == null)
                {
                    return;
                }

                var changeType = string.IsNullOrEmpty(_selectedPackage.Info.PackageVersion) ? ChangeTypes.Create : ChangeTypes.Update;

                _changes.Changes.Enqueue(
                    new PackageFieldChange(changeType, $"(P) {_selectedPackage.Info.PackageName}:{nameof(Package.Info.PackageVersion)}", _selectedPackage.Info.PackageVersion, s));
                _selectedPackage.SetPackageVersion(s);
                Console.WriteLine($"Package version update added to changeset.");
            }, verArg);


            var descArg = new Argument<string>("desc", "The new description.");
            var setDescCommand = new Command("description", "Sets the description of the package.")
            {
                descArg
            };
            setDescCommand.SetHandler(s =>
            {
                if (!CheckActivePackage() || _selectedPackage == null)
                {
                    return;
                }

                var changeType = string.IsNullOrEmpty(_selectedPackage.Info.PackageName) ? ChangeTypes.Create : ChangeTypes.Update;

                _changes.Changes.Enqueue(
                    new PackageFieldChange(changeType, $"(P) {_selectedPackage.Info.PackageName}:{nameof(Package.Info.PackageDescription)}", _selectedPackage.Info.PackageDescription, s));
                _selectedPackage.SetPackageDescription(s);
                Console.WriteLine($"Package description update added to changeset.");
            }, descArg);


            var pathArg = new Argument<string>("path", "The new filepath.");
            var setFileCommand = new Command("file", "Sets the filepath of the package.")
            {
                pathArg
            };
            setFileCommand.SetHandler(s =>
            {
                if (!CheckActivePackage() || _selectedPackage == null)
                {
                    return;
                }

                var changeType = string.IsNullOrEmpty(_selectedPackage.Info.ManifestPath) ? ChangeTypes.Create : ChangeTypes.Update;

                _changes.Changes.Enqueue(
                    new PackageFieldChange(changeType, $"(P) {_selectedPackage.Info.PackageName}:{nameof(Package.Info.ManifestPath)}", _selectedPackage.Info.ManifestPath, s));
                _selectedPackage.SetPackagePath(s);
                Console.WriteLine($"Package file update added to changeset.");
            }, pathArg);






            var keyArg = new Argument<string>("name", "The name of the variable to set or create.");
            var valueArg = new Argument<string>("value", "The variable's new value.");
            var setVarCommand = new Command("variable", "Sets a package variable.")
            {
                keyArg,
                valueArg
            };
            setVarCommand.SetHandler((k, v) =>
            {
                if (!CheckActivePackage() || _selectedPackage == null)
                {
                    return;
                }

                var existingVar = _selectedPackage.Info.PackageVariables.FirstOrDefault(v => v.Key == k);
                var existingValue = _selectedPackage.Info.PackageVariables.Any(v => v.Key == k) ? existingVar.Value : null;

                ChangeTypes type = string.IsNullOrEmpty(existingValue) ? ChangeTypes.Create : ChangeTypes.Update;

                _changes.Changes.Enqueue(
                    new PackageFieldChange(type, $"(P) {_selectedPackage.Info.PackageName}:{nameof(Package.Info.PackageVariables)}:{k}", existingValue ?? "", v));
                _selectedPackage.SetPackageVariable(k, v);

                if(type == ChangeTypes.Create)
                {
                    Console.WriteLine($"New package variable added to changeset.");
                }
                else
                {
                    Console.WriteLine($"Package variable update added to changeset.");
                }
            }, keyArg, valueArg);



            var setPackageCommand = new Command("package")
            {
                setNameCommand,
                setVerCommand,
                setDescCommand,
                setFileCommand,
                setVarCommand,
            };



            return setPackageCommand;
        }


        private static Command GetSetModuleCommand()
        {
            var nameArg = new Argument<string>("value", "The new name.");
            var setNameCommand = new Command("name", "Sets the name of the module.")
            {
                nameArg
            };
            setNameCommand.SetHandler(s =>
            {
                if (!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }
                _changes.Changes.Enqueue(new ModuleChange(ChangeTypes.Update, _selectedModule.ModuleInfo.ScriptName, s));
                _selectedModule.SetModuleName(s);
                Console.WriteLine($"Module name update added to changeset.");
            }, nameArg);


            var verArg = new Argument<string>("ver", "The new version.");
            var setVerCommand = new Command("version", "Sets the version of the module.")
            {
                verArg
            };
            setVerCommand.SetHandler(s =>
            {
                if (!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }

                var changeType = string.IsNullOrEmpty(_selectedModule.ModuleInfo.ScriptVersion) ? ChangeTypes.Create : ChangeTypes.Update;

                _changes.Changes.Enqueue(
                    new ModuleFieldChange(changeType, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.ScriptVersion)}", _selectedModule.ModuleInfo.ScriptVersion, s));
                _selectedModule.SetModuleVersion(s);

                if (changeType == ChangeTypes.Create)
                {
                    Console.WriteLine($"Module version added to changeset.");
                }
                else
                {
                    Console.WriteLine($"Module version update added to changeset.");
                }
            }, verArg);


            var descArg = new Argument<string>("desc", "The new description.");
            var setDescCommand = new Command("description", "Sets the description of the module.")
            {
                descArg
            };
            setDescCommand.SetHandler(s =>
            {
                if (!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }

                var changeType = string.IsNullOrEmpty(_selectedModule.ModuleInfo.ScriptDescription) ? ChangeTypes.Create : ChangeTypes.Update;

                _changes.Changes.Enqueue(
                    new ModuleFieldChange(changeType, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.ScriptDescription)}", _selectedModule.ModuleInfo.ScriptDescription, s));
                _selectedModule.SetModuleDescription(s);

                if (changeType == ChangeTypes.Create)
                {
                    Console.WriteLine($"Module description added to changeset.");
                }
                else
                {
                    Console.WriteLine($"Module description update added to changeset.");
                }
            }, descArg);


            var pathArg = new Argument<string>("path", "The new filepath.");
            var setFileCommand = new Command("source", "Sets the filepath of the source file.")
            {
                pathArg
            };
            setFileCommand.SetHandler(s =>
            {
                if (!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }

                var changeType = string.IsNullOrEmpty(_selectedModule.ModuleInfo.SourcePath) ? ChangeTypes.Create : ChangeTypes.Update;

                _changes.Changes.Enqueue(
                    new ModuleFieldChange(changeType, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.SourcePath)}", _selectedModule.ModuleInfo.SourcePath, s));
                _selectedModule.SetModuleSourceFile(s);

                if(changeType == ChangeTypes.Create)
                {
                    Console.WriteLine($"Module source file added to changeset.");
                }
                else
                {
                    Console.WriteLine($"Module source file update added to changeset.");
                }
            }, pathArg);


            var keyArg = new Argument<string>("name", "The name of the variable to set or create.");
            var valueArg = new Argument<string>("value", "The variable's new value.");
            var setVarCommand = new Command("variable", "Sets a module variable.")
            {
                keyArg,
                valueArg
            };
            setVarCommand.SetHandler((k, v) =>
            {
                if (!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }

                var existingVar = _selectedModule.ModuleInfo.Variables.FirstOrDefault(v => v.Key == k);
                var existingValue = _selectedModule.ModuleInfo.Variables.Any(v => v.Key == k) ? existingVar.Value : null;

                ChangeTypes type = string.IsNullOrEmpty(existingValue) ? ChangeTypes.Create : ChangeTypes.Update;

                _changes.Changes.Enqueue(
                    new PackageFieldChange(type, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.Variables)}:{k}", existingValue ?? "", v));
                _selectedModule.SetModuleVariable(k, v);

                if (type == ChangeTypes.Create)
                {
                    Console.WriteLine($"New module variable added to changeset.");
                }
                else
                {
                    Console.WriteLine($"Module variable update added to changeset.");
                }
            }, keyArg, valueArg);



            var engineArg = new Argument<string>("name", "The name or ID of the script engine.");
            var setEngineCommand = new Command("engine", "Sets module's script engine.")
            {
                engineArg
            };
            setEngineCommand.SetHandler(s =>
            {
                if (!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }

                var changeType = string.IsNullOrEmpty(_selectedModule.ModuleInfo.ScriptEngineId) ? ChangeTypes.Create : ChangeTypes.Update;

                _changes.Changes.Enqueue(
                    new ModuleFieldChange(changeType, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.ScriptEngineId)}", _selectedModule.ModuleInfo.ScriptEngineId, s));
                _selectedModule.SetModuleScriptEngine(s);

                if(changeType == ChangeTypes.Create)
                {
                    Console.WriteLine("Module script engine added to changeset.");
                }
                else
                {
                    Console.WriteLine("Module script engine update added to changeset.");
                }
            }, engineArg);


            var engineKeyArg = new Argument<string>("name", "The name of the engine argument to set or create.");
            var engineValueArg = new Argument<string>("value", "The argument's new value.");
            var setEngineVarCommand = new Command("scriptarg", "Sets an argument on the ScriptEngine.")
            {
                engineKeyArg,
                engineValueArg
            };
            setEngineVarCommand.SetHandler((k, v) =>
            {
                if (!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }

                var existingVar = _selectedModule.ModuleInfo.ScriptEngineArgs.FirstOrDefault(v => v.Key == k);
                var existingValue = _selectedModule.ModuleInfo.ScriptEngineArgs.Any(v => v.Key == k) ? existingVar.Value : null;

                ChangeTypes type = string.IsNullOrEmpty(existingValue) ? ChangeTypes.Create : ChangeTypes.Update;

                _changes.Changes.Enqueue(
                    new PackageFieldChange(type, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.ScriptEngineArgs)}:{k}", existingValue ?? "", v));
                _selectedModule.SetModuleScriptEngineArg(k, v);

                if (type == ChangeTypes.Create)
                {
                    Console.WriteLine($"New script engine argument added to changeset.");
                }
                else
                {
                    Console.WriteLine($"Script engine argument update added to changeset.");
                }
            }, keyArg, valueArg);




            var settingNameArg = new Argument<string>("name", "The name of the setting to manipulate.");
            var descriptionOption = new Option<string>(new[] { "--description", "-d" }, "The setting's description.");
            var defaultValueOption = new Option<string>(new[] { "--default-value", "-v" }, "The setting's default value.");
            var typeOption = new Option<string>(new[] { "--type", "-t" }, "The setting's type.");
            var requiredOption = new Option<bool>(new[] { "--required", "-r" }, "Whether or not the setting is required.");
            var tracked = new Option<bool>(new[] { "--tracked", "-k" }, "Whether or not the setting is tracked.");

            var setSettingCommand = new Command("setting", "Sets setting properties.")
            {
                settingNameArg,
                descriptionOption,
                defaultValueOption,
                typeOption,
                requiredOption,
                tracked
            };
            setSettingCommand.SetHandler((n, d, v, t, r, k) =>
            {
                if (!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }

                var curSetting = _selectedModule.ModuleSettings.FirstOrDefault(s => s.SettingName == n);

                if(curSetting == null)
                {
                    Console.WriteLine("That setting does not exist. First add it with 'add module setting'.");
                    return;
                }

                bool descChanged = !string.IsNullOrEmpty(d);
                bool defChanged = !string.IsNullOrEmpty(v);
                bool typeChanged = !string.IsNullOrEmpty(t);
                bool requiredChanged = curSetting.Required != r;
                bool trackedChanged = curSetting.Tracked != k;

                var descType = !descChanged ? ChangeTypes.Create : ChangeTypes.Update;
                var defType = !defChanged ? ChangeTypes.Create : ChangeTypes.Update;
                var typeType = !typeChanged ? ChangeTypes.Create : ChangeTypes.Update;
                var requiredType = ChangeTypes.Update;
                var trackedType = ChangeTypes.Update;

                if(descChanged)
                {
                    _changes.Changes.Enqueue(
                        new ModuleFieldChange(descType,
                        $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{n}:{nameof(ModuleSetting.SettingDescription)}",
                        curSetting.SettingDescription, d));

                    Console.WriteLine("Module setting's description change added to changeset.");
                }

                if(defChanged)
                {
                    _changes.Changes.Enqueue(
                        new ModuleFieldChange(defType,
                        $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{n}:{nameof(ModuleSetting.DefaultValue)}",
                        curSetting.DefaultValue ?? "", v));

                    Console.WriteLine("Module setting's default value change added to changeset.");
                }

                if(typeChanged)
                {
                    _changes.Changes.Enqueue(
                        new ModuleFieldChange(typeType,
                        $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{n}:{nameof(ModuleSetting.SettingType)}",
                        curSetting.SettingType, t));

                    Console.WriteLine("Module setting's type change added to changeset.");
                }


                if(requiredChanged)
                {
                    _changes.Changes.Enqueue(
                        new ModuleFieldChange(requiredType,
                        $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{n}:{nameof(ModuleSetting.Required)}",
                        curSetting.Required.ToString(), r.ToString()));

                    Console.WriteLine("Module setting's required change added to changeset.");
                }


                if(trackedChanged)
                {
                    _changes.Changes.Enqueue(
                        new ModuleFieldChange(trackedType,
                        $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{n}:{nameof(ModuleSetting.Tracked)}",
                        curSetting.Tracked.ToString(), k.ToString()));

                    Console.WriteLine("Module setting's tracked change added to changeset.");
                }

                _selectedModule.SetSettingDescription(n, d);
                _selectedModule.SetSettingDefault(n, v);
                _selectedModule.SetSettingType(n, t);
                _selectedModule.SetSettingRequired(n, r);
                _selectedModule.SetSettingTracked(n, k);

            }, settingNameArg, descriptionOption, defaultValueOption, typeOption, requiredOption, tracked);


            var setModuleCommand = new Command("module")
            {
                setNameCommand,
                setVerCommand,
                setDescCommand,
                setFileCommand,
                setVarCommand,
                setEngineCommand,
                setEngineVarCommand,
                setSettingCommand
            };



            return setModuleCommand;
        }

        private static Command GetAddModuleCommand()
        {
            var apiArg = new Argument<string>("id", "The host API to add.");
            var addApiCommand = new Command("api", "Adds a Host API for import to the script environment.")
            {
                apiArg
            };
            addApiCommand.SetHandler(s =>
            {
                if (!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }
                _changes.Changes.Enqueue(
                    new ModuleFieldChange(ChangeTypes.Create, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.HostApis)}", "", s));
                
                _selectedModule.AddModuleHostApi(s);

                Console.WriteLine($"Module Host API added to changeset.");
            }, apiArg);



            var settingNameArg = new Argument<string>("id", "The name of the new setting.");
            var addSettingCommand = new Command("setting", "Adds a new setting to the module.")
            {
                settingNameArg
            };
            addSettingCommand.SetHandler(s =>
            {
                if (!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }

                _changes.Changes.Enqueue(
                    new ModuleFieldChange(ChangeTypes.Create, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{s}", "", s));

                _selectedModule.AddModuleSetting(s);

                Console.WriteLine($"Module setting added to changeset.");
            }, settingNameArg);


            var bindingSettingArg = new Argument<string>("setting", "The name of the setting to bind.");
            var bindingPropertyArg = new Argument<string>("property", "The name of the binding's property.");
            var bindingTypeArg = new Argument<string>("type", "The name of the binding's type.");
            var addBindingCommand = new Command("binding", "Adds a new setting binding to the module.")
            {
                bindingSettingArg,
                bindingPropertyArg,
                bindingTypeArg
            };
            addBindingCommand.SetHandler((s, p, t) =>
            {
                if (!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }

                _changes.Changes.Enqueue(
                    new ModuleFieldChange(ChangeTypes.Create, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{s}:{p}:{t}", "", $"{p}:{t}"));

                _selectedModule.AddSettingBinding(s, p, t);

                Console.WriteLine($"Module binding added to changeset.");
            }, bindingSettingArg, bindingPropertyArg, bindingTypeArg);



            var packageOpt = new Option<string>(new[] { "--package", "-p" } , "The name or number of the package to add the module to.");
            var addToPackageCommand = new Command("topackage", "Adds or clones the active module to a package.")
            {
                packageOpt
            };
            addToPackageCommand.SetHandler(p =>
            {
                if (!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }

                Package? targetPackage = _selectedPackage;
                if(!string.IsNullOrEmpty(p))
                {
                    if (int.TryParse(p, out var i))
                    {
                        targetPackage = _packages[i];
                    }
                    else
                    {
                        targetPackage = _packages.FirstOrDefault(pk => pk.Info.PackageName == p);
                    }
                    if(targetPackage == null)
                    {
                        Console.WriteLine("Package not found.");
                        return;
                    }
                }

                if(targetPackage == null)
                {
                    Console.WriteLine("No package selected or specified.");
                    return;
                }

                _changes.Changes.Enqueue(
                    new ModuleFieldChange(ChangeTypes.Create, $"(P) {targetPackage.Info.PackageName}:{nameof(Package.DeclaredModules)}", "", $"{_selectedModule.ModuleInfo.ScriptName}"));

                targetPackage.AddModule(_selectedModule);
                Console.WriteLine("Module added to package changeset.");
            }, packageOpt);



            var addModuleCommand = new Command("module")
            {
                addApiCommand,
                addSettingCommand,
                addBindingCommand,
                addToPackageCommand
            };

            return addModuleCommand;
        }

        private static Command GetRemovePackageCommand()
        {
            var keyArg = new Argument<string>("name", "The name of the variable to remove.");
            var rmVarCommand = new Command("variable", "Removes the package variable of the specified name.")
            {
                keyArg,
            };
            rmVarCommand.SetHandler(k =>
            {
                if (!CheckActivePackage() || _selectedPackage == null)
                {
                    return;
                }

                if (!_selectedPackage.Info.PackageVariables.Any(v => v.Key == k))
                {
                    Console.WriteLine("A variable with that name could not be found.");
                    return;
                }
                var existingVar = _selectedPackage.Info.PackageVariables.FirstOrDefault(v => v.Key == k);


                _changes.Changes.Enqueue(
                    new PackageFieldChange(ChangeTypes.Delete, $"(P) {_selectedPackage.Info.PackageName}:{nameof(Package.Info.PackageVariables)}:{k}", existingVar.Value, ""));
                _selectedPackage.RemovePackageVariable(k);

                Console.WriteLine($"Package variable removal added to changeset.");
            }, keyArg);


            var packageCommand = new Command("package")
            {
                rmVarCommand
            };

            return packageCommand;
        }

        private static Command GetRemoveModuleCommand()
        {

            var engineKeyArg = new Argument<string>("name", "The name of the engine argument to remove.");
            var rmEngineVarCommand = new Command("scriptarg", "Removes an argument from the ScriptEngine.")
            {
                engineKeyArg,
            };
            rmEngineVarCommand.SetHandler(k =>
            {
                if (!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }

                var existingVar = _selectedModule.ModuleInfo.ScriptEngineArgs.FirstOrDefault(v => v.Key == k);
                var existingValue = _selectedModule.ModuleInfo.ScriptEngineArgs.Any(v => v.Key == k) ? existingVar.Value : null;

                _changes.Changes.Enqueue(
                    new PackageFieldChange(ChangeTypes.Delete, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.ScriptEngineArgs)}:{k}", existingValue, ""));
                _selectedModule.RemoveModuleScriptEngineArg(k);

                Console.WriteLine($"Script engine argument removed from changeset.");
            }, engineKeyArg);

            
            
            var apiRmArg = new Argument<string>("id", "The host API to remove.");
            var rmApiCommand = new Command("api", "Removes a Host API from the module's script environment.")
            {
                apiRmArg
            };
            rmApiCommand.SetHandler(s =>
            {
                if (!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }

                _changes.Changes.Enqueue(
                    new ModuleFieldChange(ChangeTypes.Delete, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.HostApis)}", s, ""));

                _selectedModule.RemoveModuleHostApi(s);

                Console.WriteLine($"Module Host API remove added to changeset.");
            }, apiRmArg);


            var settingArg = new Argument<string>("name", "The name of the setting.");
            var rmSettingCommand = new Command("setting", "Removes a setting from the module.")
            {
                settingArg
            };
            rmSettingCommand.SetHandler(s =>
            {
                if (!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }

                _changes.Changes.Enqueue(
                    new ModuleFieldChange(ChangeTypes.Delete, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{s}", $"{s}", ""));

                _selectedModule.RemoveModuleSetting(s);

                Console.WriteLine($"Module setting removed from changeset.");
            }, settingArg);


            var bindingSettingArg = new Argument<string>("setting", "The name of the setting to unbind.");
            var bindingPropertyArg = new Argument<string>("property", "The name of the binding's property.");
            var bindingTypeArg = new Argument<string>("type", "The name of the binding's type.");
            var rmBindingCommand = new Command("binding", "Removes a setting binding from the module.")
            {
                bindingSettingArg,
                bindingPropertyArg,
                bindingTypeArg
            };
            rmBindingCommand.SetHandler((s, p, t) =>
            {
                if (!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }

                _changes.Changes.Enqueue(
                    new ModuleFieldChange(ChangeTypes.Delete, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{s}:{p}:{t}", $"{p}:{t}", ""));

                _selectedModule.AddSettingBinding(s, p, t);

                Console.WriteLine($"Module binding removed from changeset.");
            }, bindingSettingArg, bindingPropertyArg, bindingTypeArg);


            var packageOpt = new Option<string>(new[] { "--package", "-p" });
            var rmFromPackageCommand = new Command("frompackage", "Removes a module from the package.")
            {
                packageOpt
            };
            rmFromPackageCommand.SetHandler(p =>
            {
                if(!CheckActiveModule() || _selectedModule == null)
                {
                    return;
                }

                Package? targetPackage = _selectedPackage;
                if (!string.IsNullOrEmpty(p))
                {
                    if (int.TryParse(p, out var i))
                    {
                        targetPackage = _packages[i];
                    }
                    else
                    {
                        targetPackage = _packages.FirstOrDefault(pk => pk.Info.PackageName == p);
                    }
                    if (targetPackage == null)
                    {
                        Console.WriteLine("Package not found.");
                        return;
                    }
                }

                if (targetPackage == null)
                {
                    Console.WriteLine("No package selected or specified.");
                    return;
                }

                _changes.Changes.Enqueue(
                    new ModuleFieldChange(ChangeTypes.Delete, $"(P) {targetPackage.Info.PackageName}:{nameof(Package.DeclaredModules)}", _selectedModule.ModuleInfo.ScriptName, ""));

                targetPackage.RemoveModule(_selectedModule.ModuleInfo.ScriptName);
                Console.WriteLine("Module removed from package changeset.");
            }, packageOpt);



            var rmModuleCommand = new Command("module")
            {
                rmEngineVarCommand,
                rmApiCommand,
                rmSettingCommand,
                rmBindingCommand,
                rmFromPackageCommand
            };

            return rmModuleCommand;
        }

        private static bool CheckActivePackage()
        {
            if(_selectedPackage == null)
            {
                Console.WriteLine("You must execute `use package` before using that command.");
            }

            return _selectedPackage != null;
        }

        private static bool CheckActiveModule()
        {
            if (_selectedModule == null)
            {
                Console.WriteLine("You must execute `use module` before using that command.");
            }

            return _selectedModule != null;
        }

        private static void AddCompletions(Command command, List<CompletionTree> collection)
        {
            var completionItem = new CompletionTree(command.Name);
            foreach (var item in command)
            {
                if (item is Command c)
                {
                    AddCompletions(c, completionItem.Completions);
                }
                else
                {
                    completionItem.Completions.Add(new CompletionTree(item.Name));
                }
            }
            collection.Add(completionItem);
        }

        private static void AddKeywords()
        {
            foreach (var pkg in _packages)
            {
                AddKeyword(pkg.Info.PackageName);

                foreach (var mod in pkg.DeclaredModules)
                {
                    AddKeyword(mod.ModuleInfo.ScriptName);
                }
            }
        }

        private static void AddKeyword(string kw)
        {
            const ConsoleColor KW_COLOR = ConsoleColor.Blue;
            _helper.Keywords.Add(new Keyword(kw, KW_COLOR));
        }

        private static void SetPrompt()
        {
            var pkgRun = new PromptRun(_selectedPackage?.Info.PackageName ?? "_", ConsoleColor.Yellow);
            var modRun = new PromptRun(_selectedModule?.ModuleInfo.ScriptName ?? "_", ConsoleColor.Yellow);

            _helper.Prompt = new Prompt(pkgRun, " / ", modRun, " >> ");
        }
    }
}