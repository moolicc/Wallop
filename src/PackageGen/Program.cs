
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

            foreach (var pkg in _packages)
            {
                IDHelper.Register(pkg);
                foreach (var mod in pkg.DeclaredModules)
                {
                    IDHelper.Register(mod);
                }
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
                var curColor = Console.ForegroundColor;

                if (_selectedPackage == null || a)
                {
                    int moduleCount = 0;
                    foreach (var pkg in _packages)
                    {
                        Console.WriteLine($"Modules from {pkg.Info.PackageName}");
                        foreach (var module in pkg.DeclaredModules)
                        {
                            if(module == _selectedModule)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                            }
                            Console.WriteLine("{0}: {1}", moduleCount, module.ModuleInfo.ScriptName);
                            Console.ForegroundColor = curColor;

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
                        if (modules[i] == _selectedModule)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        Console.WriteLine("{0}: {1}", i, modules[i].ModuleInfo.ScriptName);
                        Console.ForegroundColor = curColor;
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
                        _selectedPackage = MutationExtensions.CreateEmptyPackage(s);
                        int pkgId = IDHelper.Register(_selectedPackage);

                        //_changes.AddChange(new Change(ChangeTypes.Create, $"(P {pkgId}) {nameof(Package)}", "", s));
                        _packages.Add(_selectedPackage);
                        Console.WriteLine($"New package '{s}' created.");

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
                        int modId = IDHelper.Register(_selectedModule);

                        if (_selectedPackage == null)
                        {
                            //_changes.AddChange(new Change(ChangeTypes.Create, $"(M {modId}) {nameof(Module)}", "", s));
                        }
                        else
                        {
                            //_changes.AddChange(new Change(ChangeTypes.Create, nameof(Package.DeclaredModules), "", s));
                        }

                        Console.WriteLine($"New module '{s}' created.");
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
            useCommand.AddAlias("select");

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



            var changeIndex = new Argument<int>("change", "The ID of the change to undo.");
            var recursiveOpt = new Option<bool>(new[] {"--recursive", "-r"}, () => false, "Whether or not to recursively undo back to the target change.");
            var changesUndoCommand = new Command("undo", "Reverts the specified change.")
            {
                changeIndex,
                recursiveOpt
            };
            changesUndoCommand.AddAlias("revert");
            changesUndoCommand.SetHandler((id, r) =>
            {
                int changeIndex = _changes.FirstOrDefault(cg => cg.ID == id)?.ID ?? int.MaxValue;
                int startIndex = changeIndex;
                if(r)
                {
                    startIndex = 0;
                }

                for (int i = startIndex; i <= changeIndex; i++)
                {
                    if (!_changes[i].CanRevert)
                    {
                        Console.WriteLine("Change with ID {0} cannot be reverted!", _changes[i].ID);
                        break;
                    }

                    var revertChange = _changes[i].GetReversion();
                    revertChange.RevertsID = _changes[i].ID;

                    int newId = _changes.AddChange(revertChange);
                    Console.WriteLine("Revert change added for change {0}. Revert change ID: {1}", _changes[i].ID, newId);
                }
                SetPrompt();
            }, changeIndex, recursiveOpt);



            var filterOption = new Option<string>(new[] { "--filter", "-f" }, "Filters output.");
            var changesCommand = new Command("changes", "Lists the current changes that are pending.")
            {
                filterOption
            };
            changesCommand.SetHandler(f =>
            {
                _changes.PrintChangeSet(f);
                // _changes.PrintChangeSet(f);
            }, filterOption);



            var startArg = new Option<int>(new[] { "--startid", "-s" }, () => -1, "Specifies the ID of the change to start applying with.");
            var endArg = new Option<int>(new[] { "--endid", "-e" }, () => int.MaxValue, "Specifies the final ID of the change to apply.");

            var applyChanges = new Command("apply", "Applies all or some current changes to memory.")
            {
                startArg,
                endArg
            };
            applyChanges.SetHandler((s, e) =>
            {
                ApplyChanges(s, e);
                SetPrompt();
            }, startArg, endArg);


            var saveCommand = new Command("save", "Applies and saves all changes to disk.");
            saveCommand.SetHandler(_ =>
            {
                ApplyChanges();
                SavePackages();
                SetPrompt();
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
                changesUndoCommand,
                applyChanges,
                saveCommand,
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
                var curPkgId = IDHelper.FindPackageId(_selectedPackage);
                _changes.AddChange(new Change(ChangeTypes.Update, $"(P {curPkgId}) {_selectedPackage.Info.PackageName}:{nameof(Package.Info.PackageName)}", _selectedPackage.Info.PackageName, s));

                Console.WriteLine($"Package name update added to changeset.");
                SetPrompt();
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


                var curPkgId = IDHelper.FindPackageId(_selectedPackage);
                _changes.AddChange(new Change(ChangeTypes.Update, $"(P {curPkgId}) {_selectedPackage.Info.PackageName}:{nameof(Package.Info.PackageVersion)}",
                    _selectedPackage.Info.PackageVersion, s));

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


                var curPkgId = IDHelper.FindPackageId(_selectedPackage);
                _changes.AddChange(new Change(ChangeTypes.Update, $"(P {curPkgId}) {_selectedPackage.Info.PackageName}:{nameof(Package.Info.PackageDescription)}",
                    _selectedPackage.Info.PackageDescription, s));

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


                var curPkgId = IDHelper.FindPackageId(_selectedPackage);
                _changes.AddChange(new Change(ChangeTypes.Update, $"(P {curPkgId}) {_selectedPackage.Info.PackageName}:{nameof(Package.Info.ManifestPath)}",
                    _selectedPackage.Info.ManifestPath, s));

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

                var curPkgId = IDHelper.FindPackageId(_selectedPackage);
                _changes.AddChange(new Change(type, $"(P {curPkgId}) {_selectedPackage.Info.PackageName}:{nameof(Package.Info.PackageVariables)}:{k}", existingValue ?? "", v));


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
            setPackageCommand.AddAlias("pkg");


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

                var curModId = IDHelper.FindModuleId(_selectedModule);
                _changes.AddChange(new Change(ChangeTypes.Update, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.ScriptName)}",
                    _selectedModule.ModuleInfo.ScriptName, s));

                Console.WriteLine($"Module name update added to changeset.");
                SetPrompt();
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

                var curModId = IDHelper.FindModuleId(_selectedModule);
                _changes.AddChange(new Change(changeType, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.ScriptVersion)}",
                    _selectedModule.ModuleInfo.ScriptVersion, s));

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


                var curModId = IDHelper.FindModuleId(_selectedModule);
                _changes.AddChange(new Change(changeType, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.ScriptDescription)}",
                    _selectedModule.ModuleInfo.ScriptDescription, s));

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

                var curModId = IDHelper.FindModuleId(_selectedModule);
                _changes.AddChange(new Change(changeType, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.SourcePath)}",
                    _selectedModule.ModuleInfo.SourcePath, s));

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


                var curModId = IDHelper.FindModuleId(_selectedModule);
                _changes.AddChange(new Change(type, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.Variables)}:{k}", existingValue ?? "", v));


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


                var curModId = IDHelper.FindModuleId(_selectedModule);
                _changes.AddChange(new Change(changeType, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(_selectedModule.ModuleInfo.ScriptEngineId)}",
                    _selectedModule.ModuleInfo.ScriptEngineId, s));


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
            var setEngineVarCommand = new Command("enginearg", "Sets an argument on the ScriptEngine.")
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


                var curModId = IDHelper.FindModuleId(_selectedModule);
                _changes.AddChange(new Change(type, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.ScriptEngineArgs)}:{k}", existingValue ?? "", v));

                if (type == ChangeTypes.Create)
                {
                    Console.WriteLine($"New script engine argument added to changeset.");
                }
                else
                {
                    Console.WriteLine($"Script engine argument update added to changeset.");
                }
            }, engineKeyArg, engineValueArg);



            // UNTESTED
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

                var curModId = IDHelper.FindModuleId(_selectedModule);
                if (descChanged)
                {
                    _changes.AddChange(new Change(descType, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{n}:{nameof(ModuleSetting.SettingDescription)}",
                        curSetting.SettingDescription, d));

                    Console.WriteLine("Module setting's description change added to changeset.");
                }

                if(defChanged)
                {
                    _changes.AddChange(new Change(defType, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{n}:{nameof(ModuleSetting.DefaultValue)}",
                        curSetting.DefaultValue ?? "", v));

                    Console.WriteLine("Module setting's default value change added to changeset.");
                }

                if(typeChanged)
                {
                    _changes.AddChange(new Change(typeType, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{n}:{nameof(ModuleSetting.SettingType)}",
                        curSetting.SettingType, t));

                    Console.WriteLine("Module setting's type change added to changeset.");
                }


                if(requiredChanged)
                {
                    _changes.AddChange(new Change(requiredType, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{n}:{nameof(ModuleSetting.Required)}",
                        curSetting.Required, r));

                    Console.WriteLine("Module setting's required change added to changeset.");
                }


                if(trackedChanged)
                {
                    _changes.AddChange(new Change(trackedType, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{n}:{nameof(ModuleSetting.Tracked)}",
                        curSetting.Tracked, k));

                    Console.WriteLine("Module setting's tracked change added to changeset.");
                }

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
            setModuleCommand.AddAlias("mod");



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
                var curModId = IDHelper.FindModuleId(_selectedModule);
                _changes.AddChange(new Change(ChangeTypes.Create, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.HostApis)}", "", s));

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

                var curModId = IDHelper.FindModuleId(_selectedModule);
                _changes.AddChange(new Change(ChangeTypes.Create, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}", "", s));

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

                var curModId = IDHelper.FindModuleId(_selectedModule);
                _changes.AddChange(new Change(ChangeTypes.Create, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{s}:{nameof(ModuleSetting.Bindings)}", "", $"{p}:{t}"));

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

                var curPkgId = IDHelper.FindPackageId(targetPackage);
                _changes.AddChange(new Change(ChangeTypes.Update, $"(P {curPkgId}) {targetPackage.Info.PackageName}:{nameof(Package.DeclaredModules)}", "", _selectedModule));

                _selectedPackage = targetPackage;
                SetPrompt();

                Console.WriteLine("Module added to package changeset.");
            }, packageOpt);



            var addModuleCommand = new Command("module")
            {
                addApiCommand,
                addSettingCommand,
                addBindingCommand,
                addToPackageCommand
            };
            addModuleCommand.AddAlias("mod");

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

                var curPkgId = IDHelper.FindPackageId(_selectedPackage);
                _changes.AddChange(new Change(ChangeTypes.Delete, $"(P {curPkgId}) {_selectedPackage.Info.PackageName}:{nameof(Package.Info.PackageVariables)}:{k}", existingVar, ""));

                Console.WriteLine($"Package variable removal added to changeset.");
            }, keyArg);


            var packageCommand = new Command("package")
            {
                rmVarCommand
            };
            packageCommand.AddAlias("pkg");

            return packageCommand;
        }

        private static Command GetRemoveModuleCommand()
        {

            var engineKeyArg = new Argument<string>("name", "The name of the engine argument to remove.");
            var rmEngineVarCommand = new Command("enginearg", "Removes an argument from the ScriptEngine.")
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


                var curModId = IDHelper.FindModuleId(_selectedModule);
                _changes.AddChange(new Change(ChangeTypes.Delete, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.ScriptEngineArgs)}:{k}", existingValue ?? "", ""));

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

                var curModId = IDHelper.FindModuleId(_selectedModule);
                _changes.AddChange(new Change(ChangeTypes.Delete, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.HostApis)}:{s}", s, ""));

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

                var existingValue = _selectedModule.ModuleSettings.FirstOrDefault(t => t.SettingName == s);
                var curModId = IDHelper.FindModuleId(_selectedModule);
                _changes.AddChange(new Change(ChangeTypes.Delete, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{s}", existingValue?.SettingName ?? "", ""));

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

                // _changes.Changes.Enqueue(
                //    new ModuleFieldChange(ChangeTypes.Delete, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{s}:{p}:{t}", $"{p}:{t}", ""));

                var curModId = IDHelper.FindModuleId(_selectedModule);
                _changes.AddChange(new Change(ChangeTypes.Delete, $"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleSettings)}:{s}:{nameof(ModuleSetting.Bindings)}:{p},{t}", $"{p}:{t}", ""));

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

                var curPkgId = IDHelper.FindPackageId(targetPackage);
                _changes.AddChange(new Change(ChangeTypes.Delete, $"(P {curPkgId}) {targetPackage.Info.PackageName}:{nameof(Package.DeclaredModules)}:{_selectedModule.ModuleInfo.ScriptName}", _selectedModule.ModuleInfo.ScriptName, ""));

                _selectedPackage = null;
                SetPrompt();

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



        private static void ApplyChanges(int startId = -1, int endId = int.MaxValue)
        {
            var modules = new List<Module>();
            foreach (var pkg in _packages)
            {
                modules.AddRange(pkg.DeclaredModules);
            }

            for(int i = 0; i < _changes.Count(); i++)
            {
                var change = _changes[i];
                if(change.ID < startId)
                {
                    continue;
                }
                if(change.ID > endId)
                {
                    break;
                }

                var applySucceed = false;
                try
                {
                    applySucceed = change.TryApply(_packages, modules);
                    Console.Write($"Applying change #{change.ID}... ");
                }
                catch (Exception)
                {
                    applySucceed = false;
                }

                if(applySucceed)
                {
                    Console.WriteLine();
                    _changes.RemoveByIndex(i);
                    i--;
                }
                else
                {
                    Console.WriteLine($"Failed!");
                    Console.Write("Change ");
                    _changes.PrintChange(change.ID);
                    Console.WriteLine();
                    Console.Write("Continue applying changes? (y/n) ");

                    var result = Console.ReadKey();
                    Console.WriteLine();
                    if(result.Key != ConsoleKey.Y)
                    {
                        break;
                    }
                }
            }
        }

        private static void SavePackages()
        {

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
            var completionItem = new CompletionTree(command.Name, command.Aliases.ToArray());
            foreach (var item in command)
            {
                if (item is Command c)
                {
                    AddCompletions(c, completionItem.Completions);
                }
                else if(item is Option o)
                {
                    completionItem.Completions.Add(new CompletionTree(item.Name, o.Aliases.ToArray()));
                }
                else
                {
                    completionItem.Completions.Add(new CompletionTree(item.Name, Array.Empty<string>()));
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
            var runs = new List<PromptRun>();

            if(_selectedPackage != null)
            {
                var curPkgId = IDHelper.FindPackageId(_selectedPackage);
                var pkgChange = _changes.FindLastChange($"(P {curPkgId}) {_selectedPackage.Info.PackageName}:{nameof(Package.Info.PackageName)}");
                if (pkgChange == null)
                {
                    pkgChange = _changes.FindLastChange($"(P {curPkgId}) Package");
                }

                runs.Add(new PromptRun(_selectedPackage.Info.PackageName, ConsoleColor.Yellow));
                if (pkgChange != null)
                {
                    if (pkgChange.ChangeType.HasFlag(ChangeTypes.Update))
                    {
                        runs.Add(new PromptRun($" (-> {pkgChange.NewValue})", ConsoleColor.DarkCyan));
                    }
                    else if (pkgChange.ChangeType.HasFlag(ChangeTypes.Create))
                    {
                        runs.Add(new PromptRun($" (New package)", ConsoleColor.Green));
                    }
                    else if (pkgChange.ChangeType.HasFlag(ChangeTypes.Delete))
                    {
                        runs.Add(new PromptRun($" (Deleted package)", ConsoleColor.Red));
                    }
                }
            }
            else
            {
                runs.Add(new PromptRun("_", ConsoleColor.Yellow));
            }

            runs.Add(" / ");

            if (_selectedModule != null)
            {
                var curModId = IDHelper.FindModuleId(_selectedModule);
                var modChange = _changes.FindLastChange($"(M {curModId}) {_selectedModule.ModuleInfo.ScriptName}:{nameof(Module.ModuleInfo.ScriptName)}");
                if(modChange == null)
                {
                    modChange = _changes.FindLastChange($"(M {curModId}) Module");
                }

                runs.Add(new PromptRun(_selectedModule.ModuleInfo.ScriptName, ConsoleColor.Yellow));
                if (modChange != null)
                {
                    if(modChange.ChangeType.HasFlag(ChangeTypes.Update))
                    {
                        runs.Add(new PromptRun($" (-> {modChange.NewValue})", ConsoleColor.DarkCyan));
                    }
                    else if(modChange.ChangeType.HasFlag(ChangeTypes.Create))
                    {
                        runs.Add(new PromptRun($" (New module)", ConsoleColor.Green));
                    }
                    else if (modChange.ChangeType.HasFlag(ChangeTypes.Delete))
                    {
                        runs.Add(new PromptRun($" (Deleted module)", ConsoleColor.Red));
                    }
                }
            }
            else
            {
                runs.Add(new PromptRun("_", ConsoleColor.Yellow));
            }

            runs.Add(" >> ");

            _helper.Prompt = new Prompt(runs.ToArray());
        }
    }
}