using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Scripting;
using Wallop.Engine.Messaging.Messages;
using Wallop.Engine.SceneManagement;
using Wallop.Engine.SceneManagement.Serialization;
using Wallop.Engine.Scripting;
using Wallop.Engine.Scripting.ECS;
using Wallop.Engine.Settings;

namespace Wallop.Engine.Handlers
{

    internal class SceneHandler : EngineHandler
    {
        // TODO: SaveConfigMessage
        // TODO: LoadConfigMessage
        

        public PackageCache PackageCache { get; private set; }
        public SceneSettings SceneSettings => _sceneSettings;
        public Scene ActiveScene => _activeScene;

        internal SceneStore SceneStore => _sceneStore;

        private TaskHandlerProvider _taskProvider;

        private SceneSettings _sceneSettings;
        private SceneStore _sceneStore;
        private Scene _activeScene;
        private bool _sceneLoaded;

        public SceneHandler(EngineApp app, SceneSettings configuredSettings)
            : base(app)
        {
            _sceneSettings = configuredSettings;
            _sceneStore = new SceneStore();
            _sceneLoaded = false;

            PackageCache = new PackageCache(SceneSettings.PackageSearchDirectory);

            App.AddService(PackageCache);

            SubscribeToEngineMessages<SceneSettingsMessage>(HandleSceneSettings);
            SubscribeToEngineMessages<SceneChangeMessage>(HandleSceneChange);
            SubscribeToEngineMessages<AddLayoutMessage>(HandleAddLayout);
            SubscribeToEngineMessages<SetActiveLayoutMessage>(HandleSetActiveLayout);
            SubscribeToEngineMessages<AddActorMessage>(HandleAddActor);
            SubscribeToEngineMessages<CreateSceneMessage>(HandleCreateScene);
            SubscribeToEngineMessages<SceneSaveMessage>(HandleSceneSave);

            SubscribeToEngineMessages<AddDirectorMessage>(HandleAddDirector);
            SubscribeToEngineMessages<ReloadModuleMessage>(HandleReloadModule);
        }

        public override Command? GetCommandLineCommand(bool firstInstance)
        {

            var defaultSceneNameOpts = new Option<string>(
                new[] { "--default-scene", "-d" },
                () => _sceneSettings.DefaultSceneName,
                "Specifies the name of the default, empty scene");
            var drawThreadingPolicyOpts = new Option<ThreadingPolicy>(
                new[] { "--thread-policy-draw", "-tpd" },
                () => _sceneSettings.DrawThreadingPolicy,
                "Specifies the render threading policy");
            var pkgSearchDirOpts = new Option<string>(
                new[] { "--pkg-dir", "-pdir" },
                () => _sceneSettings.PackageSearchDirectory,
                "Specifies the search directory for packages");
            var scenePreloadsOpts = new Option<IEnumerable<string>>(
                new[] { "--preload", "-p" },
                () => _sceneSettings.ScenePreloadList.ToArray(),
                "Specifies the scene(s), by their json configuration filepath(s), to pre-load or reload");
            var selectedSceneOpts = new Option<string>(
                new[] { "--active-scene", "-s" },
                () => _sceneSettings.SelectedScene,
                "Specifies the currently active scene by name");
            var updateThreadingPolicyOpts = new Option<ThreadingPolicy>(
                new[] { "--thread-policy-update", "-tpu" },
                () => _sceneSettings.UpdateThreadingPolicy,
                "Specifies the update threading policy");


            var basedOnSceneOpts = new Option<string>(
                new[] { "--clone", "-c" },
                () => _sceneSettings.DefaultSceneName,
                "Bases a scene based on the scene of the specified name or configuration path");
            var newSceneNameOpts = new Argument<string>("name", 
                "The scene's name");

            var basedOnLayoutOpts = new Option<string>(
                new[] { "--clone", "-c" },
                () => "",
                "Bases a new layout on the specified existing layout");
            var newLayoutNameOpts = new Option<string>(
                new[] { "--name", "-n" },
                () => "",
                "The name of the new layout");
            var newLayoutTargetSceneOpts = new Option<string>(
                new[] { "--target-scene", "-s" },
                () => _activeScene.Name,
                "The name of the scene for which this new layout is being created");
            var makeActiveOpts = new Option<bool>(
                new[] { "--make-active", "-a" },
                () => false,
                "Whether or not to make this layout active upon creation"
                );



            var newActorNameOpts = new Option<string>(
                new[] { "--actor-name", "-n" },
                "The new actor's name");
            var newActorModuleOpts = new Option<string>(
                new[] { "--module", "-m" },
                "The new actor's module");
            var basedOnActorOpts = new Option<string>(
                new[] { "--clone-actor" },
                () => "",
                "The name of an actor to clone");
            var owningLayoutOpts = new Option<string>(
                new[] { "--layout", "-l" },
                () => _activeScene.ActiveLayout?.Name ?? "",
                "The layout upon-which to place this Actor");
            var elementSettingsOpts = new Option<Dictionary<string, string>>(
                new[] { "--settings", "-s" },
                () => new Dictionary<string, string>(),
                "A list of key/value pairs representing the actor's settings.");



            var importLocationOpts = new Argument<string>("location", "Specifies a scene configuration file to import");

            var importAsNameOpts = new Option<string>(
                new[] { "--name", "-n" },
                () => "",
                "Specifies a name to apply to the imported scene");


            var exportLocationOpts = new Argument<string>("location", "The location to export the scene configuration to");

            var exportAsNameOpts = new Option<string>(
                new[] { "--name", "-n" },
                "Specifies a name to export the scene with");


            var reloadModuleIdOpts = new Argument<string>("moduleid", "The ID of the module to reload");
            var reloadModuleKeepStateOpts = new Option<bool>(
                new[] { "-s", "--keep-state" },
                () => false,
                "Specifies whether or not to retain the state of any actors using the module");




            // EngineApp.exe scene create layout ...
            var sceneCreateLayoutCommand = new Command("layout")
            {
                basedOnLayoutOpts,
                newLayoutNameOpts,
                newLayoutTargetSceneOpts,
                makeActiveOpts
            };

            sceneCreateLayoutCommand.Handler = CommandHandler.Create<string, string, string, bool>(
                (basedOnLayout, newLayoutName, newLayoutTargetScene, makeActive) =>
                {
                    var message = new AddLayoutMessage(newLayoutName, basedOnLayout, newLayoutTargetScene, makeActive);
                    App.Messenger.Put(message);
                });

            // EngineApp.exe scene create actor ...
            var sceneCreateActor = new Command("actor")
            {
                newActorNameOpts,
                newActorModuleOpts,
                basedOnActorOpts,
                owningLayoutOpts,

                elementSettingsOpts,
            };

            sceneCreateActor.SetHandler(new Action<string, string, string, string, Dictionary<string, string>> (
                (name, module, clone, layout, settings) =>
                {
                    if(_activeScene is null)
                    {
                        // TODO: Error
                        return;
                    }
                    var newActorMessage = new AddActorMessage(name, _activeScene.Name, layout, module, settings);
                    App.Messenger.Put(newActorMessage);
                }), newActorNameOpts, newActorModuleOpts, basedOnActorOpts, owningLayoutOpts, elementSettingsOpts);


            // EngineApp.exe scene create ...
            var sceneCreateCommand = new Command("create", "Creates a new scene")
            {
                sceneCreateLayoutCommand,
                sceneCreateActor,

                basedOnSceneOpts,
                newSceneNameOpts
            };


            sceneCreateCommand.SetHandler(new Action<string, string>(
                (string newSceneName, string basedOnScene) =>
                {
                    var message = new CreateSceneMessage(newSceneName, basedOnScene);
                    App.Messenger.Put(message);
                }), newSceneNameOpts, basedOnSceneOpts);


            // EngineApp.exe scene import ...
            var sceneImportCommand = new Command("import", "Imports a scene configuration")
            {
                importLocationOpts,
                importAsNameOpts,
            };

            sceneImportCommand.SetHandler(
                (string importLocation, string importName) =>
                {
                    App.Messenger.Put(new SceneChangeMessage(importLocation));
                }, importLocationOpts, importAsNameOpts);



            // EngineApp.exe scene export ...
            var sceneExportCommand = new Command("export", "Exports a scene configuration")
            {
                exportLocationOpts,
                exportAsNameOpts,
            };

            sceneExportCommand.SetHandler(
                (string exportLocationOpts, string exportName) =>
                {
                    App.Messenger.Put(new SceneSaveMessage(SettingsSaveOptions.Default, exportLocationOpts));
                }, exportLocationOpts, exportAsNameOpts);


            // EngineApp.exe scene reload module ...
            var sceneReloadModuleCommand = new Command("module", "Reloads a module")
            {
                reloadModuleIdOpts,
                reloadModuleKeepStateOpts
            };

            sceneReloadModuleCommand.SetHandler(
                (string reloadModuleId, bool keepState) =>
                {
                    App.Messenger.Put(new ReloadModuleMessage(reloadModuleId, keepState));
                }, reloadModuleIdOpts, reloadModuleKeepStateOpts);


            // EngineApp.exe scene reload ...
            var sceneReloadCommand = new Command("reload", "Reloads an element in a scene")
            {
                sceneReloadModuleCommand
            };

            // EngineApp.exe scene info
            var sceneInfoCommand = new Command("info", "Retrieves scene information");

            sceneInfoCommand.SetHandler(() =>
            {
                Console.WriteLine(_activeScene.GetFriendlyString());
            });

            // EngineApp.exe scene ...
            var sceneCommand = new Command("scene", "Scene operations")
            {
                sceneCreateCommand,
                sceneImportCommand,
                sceneExportCommand,
                sceneReloadCommand,
                sceneInfoCommand,

                defaultSceneNameOpts,
                drawThreadingPolicyOpts,
                pkgSearchDirOpts,
                scenePreloadsOpts,
                selectedSceneOpts,
                updateThreadingPolicyOpts,
            };

            sceneCommand.Handler = CommandHandler.Create<string, ThreadingPolicy, string, IEnumerable<string>, string, ThreadingPolicy>(
                (defaultSceneName, drawThreadingPolicy, pkgSearchDir, scenePreloads, selectedScene, updateThreadingPolicy) =>
                {
                    var changes = new SceneSettings()
                    {
                        DefaultSceneName = defaultSceneName,
                        DrawThreadingPolicy = drawThreadingPolicy,
                        PackageSearchDirectory = pkgSearchDir,
                        ScenePreloadList = new List<string>(scenePreloads),
                        SelectedScene = selectedScene,
                        UpdateThreadingPolicy = updateThreadingPolicy
                    };

                    if(_sceneLoaded && firstInstance)
                    {
                        App.Messenger.Put(new SceneSettingsMessage(changes));
                    }
                    else
                    {
                        _sceneSettings = changes;
                    }
                });



            return sceneCommand;
        }


        internal void InitScene()
        {
            _sceneLoaded = true;
            SetupServices();
            SetupDefaultScene();


            EngineLog.For<SceneHandler>().Info("Preloading {scenes} scene configurations...", _sceneSettings.ScenePreloadList.Count);
            foreach (var item in _sceneSettings.ScenePreloadList)
            {
                if (item == _sceneSettings.SelectedScene || item == _sceneSettings.DefaultSceneName)
                {
                    continue;
                }
                _sceneStore.Load(item);
            }

            if (_sceneSettings.SelectedScene != _sceneSettings.DefaultSceneName && !_sceneSettings.ScenePreloadList.Contains(_sceneSettings.SelectedScene))
            {
                _sceneStore.Load(_sceneSettings.SelectedScene);
            }

            SwitchScene(_sceneSettings.SelectedScene);
        }

        private void SetupServices()
        {
            _taskProvider = new TaskHandlerProvider(_sceneSettings.UpdateThreadingPolicy, _sceneSettings.DrawThreadingPolicy, () =>
            {
                //_window.MakeCurrent();
            });
        }

        private void SetupDefaultScene()
        {
            EngineLog.For<SceneHandler>().Info("Setting up default scene...");

            var defaultSettings = new StoredScene()
            {
                Name = _sceneSettings.DefaultSceneName,
                DirectorModules = new List<StoredModule>()
                {
                    //new StoredModule()
                    //{
                    //    InstanceName = "DirectorTest",
                    //    ModuleId = "Director.Test1.0",
                    //    Settings = new List<StoredSetting>()
                    //    {
                    //        new StoredSetting("height", "100"),
                    //        new StoredSetting("width", "100"),
                    //    },
                    //    //Settings = new Dictionary<string, string>()
                    //    //{
                    //    //    { "height", "100" },
                    //    //    { "width", "100" }
                    //    //},
                    //},
                },
                Layouts = new List<StoredLayout>()
                {
                    new StoredLayout()
                    {
                        Active = true,
                        Name = "layout1",
                        ActorModules = new List<StoredModule>()
                        {
                            new StoredModule()
                            {
                                InstanceName = "Square",
                                ModuleId = "Square.Test1.0",
                                Settings = new List<StoredSetting>()
                                {
                                    new StoredSetting("height", "100"),
                                    new StoredSetting("width", "100"),
                                },
                            },
                            new StoredModule()
                            {
                                InstanceName = "Square1",
                                ModuleId = "Square.Test1.0",
                                Settings = new List<StoredSetting>()
                                {
                                    new StoredSetting("height", "100"),
                                    new StoredSetting("width", "100"),
                                    new StoredSetting("y", "200"),
                                },
                                StoredBindings = new List<StoredBinding>()
                                {
                                    new StoredBinding("PositionComponent", "X", "x"),
                                    new StoredBinding("PositionComponent", "Y", "y"),
                                }
                            }
                        }
                    }
                }
            };
            _sceneStore.Add(defaultSettings);
        }

        public void SwitchScene(string newSceneNameOrConfig)
        {
            EngineLog.For<SceneHandler>().Info("Switching to new scene {scene}.", newSceneNameOrConfig);
            var settings = _sceneStore.Get(newSceneNameOrConfig);

            if (settings == null)
            {
                EngineLog.For<EngineApp>().Info("Loading scene from configuration...");
                settings = _sceneStore.Load(newSceneNameOrConfig);
            }
            if (settings == null)
            {
                EngineLog.For<EngineApp>().Error("Failed to locate scene by name or by configuration path.");
                return;
            }

            var packageCache = App.GetService<PackageCache>().OrThrow();
            var bindableComponentTypes = App.GetService<BindableComponentTypeCache>().OrThrow();
            //var taskProvider = App.GetService<TaskHandlerProvider>().OrThrow();
            var pluginContext = App.GetService<PluginPantry.PluginContext>().OrThrow();
            var hostFunctions = App.GetService<ScriptHostFunctions>().OrThrow();
            var engineProviders = App.GetService<ScriptEngineProviderCache>().OrThrow();


            EngineLog.For<SceneHandler>().Info("Initializing ECS Element invokers...");
            var elementLoader = new Scripting.ECS.Serialization.ElementLoader(packageCache);
            var elementInitializer = new Scripting.ECS.Serialization.ElementInitializer(engineProviders, _taskProvider, hostFunctions, pluginContext, bindableComponentTypes);

            EngineLog.For<SceneHandler>().Info("Constructing scene...");
            var sceneLoader = new SceneLoader(settings, packageCache);
            var scene = sceneLoader.LoadScene();


            EngineLog.For<SceneHandler>().Debug("Injecting script HostData...");
            Func<GL> glGetter = App.GetHandler<GraphicsHandler>().OrThrow().GetGlIsntance;
            var hostData = new HostData(glGetter, scene, bindableComponentTypes);
            hostFunctions.AddDependencies(hostData);



            EngineLog.For<SceneHandler>().Info("Initializing scene and associated modules...");

            scene.Init(pluginContext);
            var initializer = new SceneScriptInitializer(scene);
            initializer.InitializeActorScripts();
            initializer.InitializeDirectorScripts();

            _activeScene = scene;
            _sceneSettings.SelectedScene = scene.Name;

            App.GetHandler<GraphicsHandler>()?.Bump();
        }

        public void SaveCurrentSceneConfig(SettingsSaveOptions savePolicies, string filepath)
        {
            EngineLog.For<SceneHandler>().Info("Saving scene configuration to {location}...", filepath);
            var saver = new SceneSaver(savePolicies, PackageCache);
            var settings = saver.Save(_activeScene);
            _sceneStore.Add(settings);
            var json = _sceneStore.Save(settings.Name);
            File.WriteAllText(filepath, json);
        }

        public void ReloadModule(string moduleId, bool keepState)
        {
            EngineLog.For<SceneHandler>().Info("Reloading module '{module}'...", moduleId);
            var sceneSaver = new SceneSaver(keepState ? SettingsSaveOptions.EntireState : SettingsSaveOptions.Default, PackageCache);

            // First, find all elements using this module and save their state.
            EngineLog.For<SceneHandler>().Info("Saving directors with module '{module}'...", moduleId);

            var applicableDirectors = ActiveScene.Directors
                .Where(d => d is ScriptedDirector xD && xD.ModuleDeclaration.ModuleInfo.Id == moduleId);
            var savedDirectors = sceneSaver.SaveDirectors(applicableDirectors.Select(d => (ScriptedDirector)d));

            // Remove the directors from the scene.
            foreach (var item in applicableDirectors)
            {
                _activeScene.Directors.Remove(item);
            }

            EngineLog.For<SceneHandler>().Info("Saving actors with module '{module}'...", moduleId);
            var savedLayouts = new List<(Layout SceneLayout, StoredLayout SavedInfo)>();

            int actorCount = 0;
            foreach (var layout in ActiveScene.Layouts)
            {
                var applicableActors = layout.EcsRoot.GetActors()
                    .Where(a => a is ScriptedActor xA && xA.ModuleDeclaration.ModuleInfo.Id == moduleId);

                var savedActors = sceneSaver.SaveActors(applicableActors.Select(a => (ScriptedActor)a));

                var storedLayout = new StoredLayout();
                storedLayout.ActorModules = savedActors.ToList();

                // Remove the actors from the layout.
                foreach (var item in applicableActors)
                {
                    layout.EcsRoot.Remove(item);
                }

                actorCount += storedLayout.ActorModules.Count;

                savedLayouts.Add((layout, storedLayout));
            }


            EngineLog.For<SceneHandler>().Info("Reloading {count} directors with module '{module}'...", savedDirectors.Count(), moduleId);
            // Finally, re-init the elements using the new module.
            var storedScene = new StoredScene();
            storedScene.DirectorModules = savedDirectors.ToList();
            storedScene.Layouts = savedLayouts.Select(l => l.SavedInfo).ToList();

            var sceneLoader = new SceneLoader(storedScene, PackageCache);
            sceneLoader.CreateDirectors(ActiveScene);

            EngineLog.For<SceneHandler>().Info("Reloading {actorCount} actors within {layoutCount} layouts with module '{module}'...", actorCount, savedLayouts.Count, moduleId);
            foreach (var item in savedLayouts)
            {
                foreach (var savedActor in item.SavedInfo.ActorModules)
                {
                    var actor = Scripting.ECS.Serialization.ElementLoader.Instance.Load<ScriptedActor>(savedActor);

                    item.SceneLayout.EcsRoot.AddActor(actor);
                    actor.AddedToLayout(item.SceneLayout);

                    Scripting.ECS.Serialization.ElementInitializer.Instance.InitializeElement(actor, _activeScene);

                }
            }
        }


        private void HandleSceneSettings(SceneSettingsMessage message, uint messageId)
        {
            // Handled within the below call to SwitchScene()
            //_sceneSettings.SelectedScene = message.Settings.SelectedScene;
            _sceneSettings.DefaultSceneName = message.Settings.DefaultSceneName;

            if (_sceneSettings.PackageSearchDirectory != message.Settings.PackageSearchDirectory)
            {
                var packageCache = App.GetService<PackageCache>().OrThrow();
                _sceneSettings.PackageSearchDirectory = message.Settings.PackageSearchDirectory;
                packageCache.ReloadAll(message.Settings.PackageSearchDirectory);
            }

            if (_sceneSettings.DrawThreadingPolicy != message.Settings.DrawThreadingPolicy)
            {
                _sceneSettings.DrawThreadingPolicy = message.Settings.DrawThreadingPolicy;
                _taskProvider.SetDrawPolicy(message.Settings.DrawThreadingPolicy);
            }
            if (_sceneSettings.UpdateThreadingPolicy != message.Settings.UpdateThreadingPolicy)
            {
                _sceneSettings.UpdateThreadingPolicy = message.Settings.UpdateThreadingPolicy;
                _taskProvider.SetUpdatePolicy(message.Settings.UpdateThreadingPolicy);
            }


            foreach (var item in message.Settings.ScenePreloadList)
            {
                if (item == message.Settings.SelectedScene || item == message.Settings.DefaultSceneName)
                {
                    continue;
                }
                if (_sceneSettings.ScenePreloadList.Contains(item))
                {
                    continue;
                }
                _sceneSettings.ScenePreloadList.Add(item);
                _sceneStore.Load(item);
            }

            if(message.Settings.SelectedScene != _sceneSettings.SelectedScene)
            {
                SwitchScene(_sceneSettings.SelectedScene);
            }
        }

        private void HandleSceneChange(SceneChangeMessage message, uint messageId)
        {
            SwitchScene(message.NewScene);
        }

        private void HandleAddLayout(AddLayoutMessage message, uint messageId)
        {
            // TODO: Handle cloning layouts.

            if (message.TargetScene == null)
            {
                var newLayout = new Layout(message.Name);

                var scene = _activeScene;
                scene.Layouts.Add(newLayout);


                if (message.MakeActive)
                {
                    scene.ActiveLayout = newLayout;
                }
            }
            else
            {
                var newLayout = new SceneManagement.StoredLayout();
                newLayout.Name = message.Name;
                newLayout.Active = message.MakeActive;

                var scene = _sceneStore.Get(message.TargetScene);
                if (scene == null)
                {
                    // TODO: Error.
                    return;
                }

                if (newLayout.Active)
                {
                    foreach (var item in scene.Layouts)
                    {
                        if (item.Active)
                        {
                            item.Active = false;
                        }
                    }
                }
                scene.Layouts.Add(newLayout);
            }

        }

        private void HandleSetActiveLayout(SetActiveLayoutMessage message, uint messageId)
        {
            var layout = _activeScene.Layouts.FirstOrDefault(l => l.Name == message.LayoutName);
            if (layout == null)
            {
                // TODO: error.
                return;
            }

            _activeScene.ActiveLayout = layout;
        }

        private void HandleAddActor(AddActorMessage message, uint messageId)
        {
            // TODO: Stored bindings

            // Create an actor based on the module specified in the message and add it to the specified layout.
            var actorDefinition = new SceneManagement.StoredModule()
            {
                InstanceName = message.ActorName,
                ModuleId = message.BasedOnModule,
                Settings = StoredSetting.FromEnumerable(message.ModuleSettings ?? Array.Empty<KeyValuePair<string, string>>()).ToList(),
            };


            // If there was no specified scene, or the specified scene is active then we add the director
            // to the current scene.

            // Otherwise, we add it to the specified scene in storage.
            if (message.Scene == null || message.Scene.Equals(_activeScene.Name))
            {
                var actor = Scripting.ECS.Serialization.ElementLoader.Instance.Load<Scripting.ECS.ScriptedActor>(actorDefinition);

                Layout? layout = null;
                if (message.Layout == null)
                {
                    layout = _activeScene.ActiveLayout;
                }
                else
                {
                    foreach (var cur in _activeScene.Layouts)
                    {
                        if (cur.Name == message.Layout)
                        {
                            layout = cur;
                            break;
                        }
                    }
                }
                if (layout == null)
                {
                    throw new InvalidOperationException($"Failed to add actor. Specified layout ({message.Layout}) does not exist.");
                }

                layout.EcsRoot.AddActor(actor);
                actor.AddedToLayout(layout);
                Scripting.ECS.Serialization.ElementInitializer.Instance.InitializeElement(actor, _activeScene);
            }
            else
            {
                var scene = _sceneStore.Get(message.Scene);
                if (scene == null)
                {
                    // TODO: Error
                    return;
                }

                StoredLayout? layout = null;
                foreach (var cur in scene.Layouts)
                {
                    if (message.Layout == null && cur.Active)
                    {
                        layout = cur;
                        break;
                    }
                    else if (message.Layout != null && cur.Name == message.Layout)
                    {
                        layout = cur;
                        break;
                    }
                }

                if (layout == null)
                {
                    // TODO: Error
                    return;
                }
                layout.ActorModules.Add(actorDefinition);
            }
        }

        private void HandleCreateScene(CreateSceneMessage message, uint messageId)
        {
            var scene = new StoredScene();
            scene.Name = message.NewSceneName;
            _sceneStore.Add(scene);

            SwitchScene(message.NewSceneName);
        }

        private void HandleSceneSave(SceneSaveMessage message, uint messageId)
        {
            SaveCurrentSceneConfig(message.Options, message.Location);
        }

        private void HandleAddDirector(AddDirectorMessage message, uint messageId)
        {
            // TODO: Stored bindings

            var directorDefinition = new StoredModule()
            {
                InstanceName = message.DirectorName,
                ModuleId = message.BasedOnModule,
                Settings = StoredSetting.FromEnumerable(message.ModuleSettings ?? Array.Empty<KeyValuePair<string, string>>()).ToList(),
            };


            // If no scene was specified, or the specified scene is currently active we add the director to the
            // active scene.

            // Otherwise, we add it to the specified scene in storage.
            if (message.Scene == null || message.Scene.Equals(_activeScene.Name))
            {
                // Create and initialize the director.
                var director = Scripting.ECS.Serialization.ElementLoader.Instance.Load<ScriptedDirector>(directorDefinition);
                Scripting.ECS.Serialization.ElementInitializer.Instance.InitializeElement(director, _activeScene);

                // Add the director to the scene.
                _activeScene.Directors.Add(director);
            }
            else
            {
                // Attempt to get the scene from storage.
                var scene = _sceneStore.Get(message.Scene);
                if (scene == null)
                {
                    // TODO: Error
                    return;
                }

                // Add the director definition to the stored scene.
                scene.DirectorModules.Add(directorDefinition);
            }
        }

        public void HandleReloadModule(ReloadModuleMessage message, uint messageId)
        {
            ReloadModule(message.ModuleId, message.keepState);
        }


        public void SceneUpdate()
        {
            _activeScene.Update();
        }

        public void SceneDraw()
        {
            _activeScene.Draw();
        }

        public override void Shutdown()
        {
            _activeScene.Shutdown();
            EngineLog.For<SceneHandler>().Info("SceneHandler shutdown.");
        }
    }
}
