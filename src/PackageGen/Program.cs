
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

            command.Invoke("--help");
            while (_repl)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
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
                    }
                }
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
                    }
                }
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

        private static Command GetRemoveCommand()
        {
            var rmCommand = new Command("remove", "Performs value removal operations.")
            {
                GetRemovePackageCommand()
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

                _changes.Changes.Enqueue(
                    new PackageFieldChange(ChangeTypes.Update, $"(P) {_selectedPackage.Info.PackageName}:{nameof(_selectedPackage.Info.PackageVersion)}", _selectedPackage.Info.PackageVersion, s));
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

                _changes.Changes.Enqueue(
                    new PackageFieldChange(ChangeTypes.Update, $"(P) {_selectedPackage.Info.PackageName}:{nameof(_selectedPackage.Info.PackageDescription)}", _selectedPackage.Info.PackageDescription, s));
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

                _changes.Changes.Enqueue(
                    new PackageFieldChange(ChangeTypes.Update, $"(P) {_selectedPackage.Info.PackageName}:{nameof(_selectedPackage.Info.ManifestPath)}", _selectedPackage.Info.ManifestPath, s));
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
                    new PackageFieldChange(type, $"(P) {_selectedPackage.Info.PackageName}:{nameof(_selectedPackage.Info.PackageVariables)}:{k}", existingValue ?? "", v));
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

                _changes.Changes.Enqueue(
                    new ModuleFieldChange(ChangeTypes.Update, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(_selectedModule.ModuleInfo.ScriptVersion)}", _selectedModule.ModuleInfo.ScriptVersion, s));
                _selectedModule.SetModuleVersion(s);
                Console.WriteLine($"Module version update added to changeset.");
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

                _changes.Changes.Enqueue(
                    new ModuleFieldChange(ChangeTypes.Update, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(_selectedModule.ModuleInfo.ScriptDescription)}", _selectedModule.ModuleInfo.ScriptDescription, s));
                _selectedModule.SetModuleDescription(s);
                Console.WriteLine($"Module description update added to changeset.");
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

                _changes.Changes.Enqueue(
                    new ModuleFieldChange(ChangeTypes.Update, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(_selectedModule.ModuleInfo.SourcePath)}", _selectedModule.ModuleInfo.SourcePath, s));
                _selectedModule.SetModuleSourceFile(s);
                Console.WriteLine($"Module source file update added to changeset.");
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
                    new PackageFieldChange(type, $"(M) {_selectedModule.ModuleInfo.ScriptName}:{nameof(_selectedModule.ModuleInfo.Variables)}:{k}", existingValue ?? "", v));
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



            var setModuleCommand = new Command("module")
            {
                setNameCommand,
                setVerCommand,
                setDescCommand,
                setFileCommand,
                setVarCommand,
            };



            return setModuleCommand;
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
                    new PackageFieldChange(ChangeTypes.Delete, $"(P) {_selectedPackage.Info.PackageName}:{nameof(_selectedPackage.Info.PackageVariables)}:{k}", existingVar.Value, ""));
                _selectedPackage.RemovePackageVariable(k);

                Console.WriteLine($"Package variable removal added to changeset.");
            }, keyArg);


            var packageCommand = new Command("package")
            {
                rmVarCommand
            };

            return packageCommand;
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
    }
}