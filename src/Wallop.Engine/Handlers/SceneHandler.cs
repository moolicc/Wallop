using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Scripting;
using Wallop.Engine.Messaging.Messages;
using Wallop.Engine.SceneManagement;
using Wallop.Engine.SceneManagement.Serialization;
using Wallop.Engine.Scripting;
using Wallop.Engine.Settings;

namespace Wallop.Engine.Handlers
{
    internal class SceneHandler : EngineHandler
    {
        // TODO: SaveConfigMessage
        // TODO: LoadConfigMessage

        public SceneSettings SceneSettings => _sceneSettings;
        public Scene ActiveScene => _activeScene;

        internal SceneStore SceneStore => _sceneStore;

        private SceneSettings _sceneSettings;
        private SceneStore _sceneStore;
        private Scene _activeScene;

        public SceneHandler(EngineApp app, SceneSettings configuredSettings)
            : base(app)
        {
            _sceneSettings = configuredSettings;
            _sceneStore = new SceneStore();

            SubscribeToEngineMessages<SceneSettingsMessage>(HandleSceneSettings);
            SubscribeToEngineMessages<SceneChangeMessage>(HandleSceneChange);
            SubscribeToEngineMessages<AddLayoutMessage>(HandleAddLayout);
            SubscribeToEngineMessages<SetActiveLayoutMessage>(HandleSetActiveLayout);
            SubscribeToEngineMessages<AddActorMessage>(HandleAddActor);
            SubscribeToEngineMessages<CreateSceneMessage>(HandleCreateScene);

            SubscribeToEngineMessages<AddDirectorMessage>(HandleAddDirector);
        }

        public override Command? GetCommandLineCommand()
        {

            var defaultSceneName = new Option<string>(
                new[] { "--default-scene", "-d" },
                () => _sceneSettings.DefaultSceneName,
                "Specifies the name of the default, empty scene.");
            var drawThreadingPolicy = new Option<ThreadingPolicy>(
                new[] { "--thread-policy-draw", "-tpd" },
                () => _sceneSettings.DrawThreadingPolicy,
                "Specifies the render threading policy.");
            var pkgSearchDir = new Option<string>(
                new[] { "--pkg-dir", "-pdir" },
                () => _sceneSettings.PackageSearchDirectory,
                "Specifies the search directory for packages.");
            var scenePreloads = new Option<IEnumerable<string>>(
                new[] { "--preload", "-p" },
                () => _sceneSettings.ScenePreloadList.ToArray(),
                "Specifies the scene(s), by their json configuration filepath(s), to pre-load or reload.");
            var selectedScene = new Option<string>(
                new[] { "--active-scene", "-s" },
                () => _sceneSettings.SelectedScene,
                "Specifies the currently active scene by name.");
            var updateThreadingPolicy = new Option<ThreadingPolicy>(
                new[] { "--thread-policy-update", "-tpu" },
                () => _sceneSettings.UpdateThreadingPolicy,
                "Specifies the update threading policy.");

            var basedOnScene = new Option<string>(
                new[] { "--clone", "-c" },
                () => _sceneSettings.DefaultSceneName,
                "Bases a scene based on the scene of the specified name or configuration path.");
            var newSceneName = new Option<string>(
                new[] { "--name", "-n" },
                "Bases a scene based on the scene of the specified name or configuration path.");

            var basedOnLayout = new Option<string>(
                new[] { "--clone", "-c" },
                () => "",
                "Bases a new layout on the specified existing layout.");
            var newLayoutName = new Option<string>(
                new[] { "--name", "-n" },
                () => "",
                "The name of the new layout");
            var newLayoutTargetScene = new Option<string>(
                new[] { "--target-scene", "-s" },
                () => _activeScene.Name,
                "The name of the scene for which this new layout is being created.");
            var makeActive = new Option<bool>(
                new[] { "--make-active", "-a" },
                () => false,
                "Whether or not to make this layout active upon creation."
                );


            // EngineApp.exe scene create layout ...
            var sceneCreateLayoutCommand = new Command("layout")
            {
                basedOnLayout,
                newLayoutName,
                newLayoutTargetScene,
                makeActive
            };

            // EngineApp.exe scene create ...
            var sceneCreateCommand = new Command("create", "Creates a new scene")
            {
                sceneCreateLayoutCommand,

                basedOnScene,
                newSceneName
            };

            // EngineApp.exe scene ...
            var sceneCommand = new Command("scene", "Scene operations")
            {
                sceneCreateCommand,

                defaultSceneName,
                drawThreadingPolicy,
                pkgSearchDir,
                scenePreloads,
                selectedScene,
                updateThreadingPolicy,
            };

            return sceneCommand;
        }


        internal void InitScene()
        {
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

        private void SetupDefaultScene()
        {
            EngineLog.For<SceneHandler>().Info("Setting up default scene...");

            var defaultSettings = new StoredScene()
            {
                Name = _sceneSettings.DefaultSceneName,
                DirectorModules = new List<StoredModule>()
                {
                    new StoredModule()
                    {
                        InstanceName = "DirectorTest",
                        ModuleId = "Director.Test1.0",
                        Settings = new Dictionary<string, string>()
                        {
                            { "height", "100" },
                            { "width", "100" }
                        },
                    },
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
                                Settings = new Dictionary<string, string>()
                                {
                                    { "height", "100" },
                                    { "width", "100" }
                                }
                            },
                            new StoredModule()
                            {
                                InstanceName = "Square1",
                                ModuleId = "Square.Test1.0",
                                Settings = new Dictionary<string, string>()
                                {
                                    { "height", "100" },
                                    { "width", "100" },
                                    { "y", "200" }
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
            var bindableComponentTypes = App.GetService <IEnumerable<KeyValuePair<string, Type>>>().OrThrow();
            var taskProvider = App.GetService<TaskHandlerProvider>().OrThrow();
            var pluginContext = App.GetService<PluginPantry.PluginContext>().OrThrow();
            var hostFunctions = App.GetService<ScriptHostFunctions>().OrThrow();
            var engineProviders = App.GetService<ScriptEngineProviderCache>().OrThrow();

            EngineLog.For<SceneHandler>().Info("Constructing scene...");
            var sceneLoader = new SceneLoader(settings, packageCache);
            var scene = sceneLoader.LoadScene();


            EngineLog.For<SceneHandler>().Debug("Injecting script HostData...");
            var hostData = new HostData(App.GetHandler<GraphicsHandler>()?.GetGlIsntance(), scene, bindableComponentTypes);
            hostFunctions.AddDependencies(hostData);



            EngineLog.For<SceneHandler>().Info("Initializing scene and associated modules...");

            taskProvider = new TaskHandlerProvider(_sceneSettings.UpdateThreadingPolicy, _sceneSettings.DrawThreadingPolicy, () =>
            {
                //_window.MakeCurrent();
            });

            scene.Init(pluginContext);

            var elementLoader = new Scripting.ECS.Serialization.ElementLoader(packageCache);
            var elementInitializer = new Scripting.ECS.Serialization.ElementInitializer(engineProviders, taskProvider, hostFunctions, pluginContext, bindableComponentTypes);

            var initializer = new SceneScriptInitializer(scene);
            initializer.InitializeActorScripts();
            initializer.InitializeDirectorScripts();

            _activeScene = scene;
        }

        public void SaveCurrentSceneConfig(SettingsSaveOptions savePolicies, string filepath)
        {
            var packageCache = App.GetService<PackageCache>().OrThrow();
            var saver = new SceneSaver(savePolicies, packageCache);
            var settings = saver.Save(_activeScene);
            _sceneStore.Add(settings);
            var json = _sceneStore.Save(settings.Name);
            File.WriteAllText(filepath, json);
        }


        private void HandleSceneSettings(SceneSettingsMessage message, uint messageId)
        {
            _sceneSettings.SelectedScene = message.Settings.SelectedScene;
            _sceneSettings.DefaultSceneName = message.Settings.DefaultSceneName;

            if (_sceneSettings.PackageSearchDirectory != message.Settings.PackageSearchDirectory)
            {
                _sceneSettings.PackageSearchDirectory = message.Settings.PackageSearchDirectory;
                App.PackageCache.Reload(message.Settings.PackageSearchDirectory);
            }

            if (_sceneSettings.DrawThreadingPolicy != message.Settings.DrawThreadingPolicy)
            {
                _sceneSettings.DrawThreadingPolicy = message.Settings.DrawThreadingPolicy;
                App.TaskHandlerProvider.SetDrawPolicy(message.Settings.DrawThreadingPolicy);
            }
            if (_sceneSettings.UpdateThreadingPolicy != message.Settings.UpdateThreadingPolicy)
            {
                _sceneSettings.UpdateThreadingPolicy = message.Settings.UpdateThreadingPolicy;
                App.TaskHandlerProvider.SetUpdatePolicy(message.Settings.UpdateThreadingPolicy);
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
                var newLayout = new SceneManagement.Layout();
                newLayout.Name = message.Name;

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
                Settings = new Dictionary<string, string>(message.ModuleSettings ?? Array.Empty<KeyValuePair<string, string>>())
            };


            // If there was no specified scene, or the specified scene is active then we add the director
            // to the current scene.

            // Otherwise, we add it to the specified scene in storage.
            if (message.Scene == null || message.Scene.Equals(_activeScene.Name))
            {
                var actor = Scripting.ECS.Serialization.ElementLoader.Instance.Load<Scripting.ECS.ScriptedActor>(actorDefinition);
                Scripting.ECS.Serialization.ElementInitializer.Instance.InitializeElement(actor, _activeScene);

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
                    // TODO: Error
                    return;
                }

                layout.EcsRoot.AddActor(actor);
                actor.AddedToLayout(layout);
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
        }

        private void HandleAddDirector(AddDirectorMessage message, uint messageId)
        {
            // TODO: Stored bindings

            var directorDefinition = new StoredModule()
            {
                InstanceName = message.DirectorName,
                ModuleId = message.BasedOnModule,
                Settings = new Dictionary<string, string>(message.ModuleSettings ?? Array.Empty<KeyValuePair<string, string>>())
            };


            // If no scene was specified, or the specified scene is currently active we add the director to the
            // active scene.

            // Otherwise, we add it to the specified scene in storage.
            if (message.Scene == null || message.Scene.Equals(_activeScene.Name))
            {
                // Create and initialize the director.
                var director = Scripting.ECS.Serialization.ElementLoader.Instance.Load<Scripting.ECS.ScriptedDirector>(directorDefinition);
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

        public void SceneUpdate()
        {
            _activeScene.Update();
        }

        public void SceneDraw()
        {
            _activeScene.Draw();

        }

        internal void SceneShutdown()
        {
            _activeScene.Shutdown();
            EngineLog.For<SceneHandler>().Info("Scene shutdown.");
        }
    }
}
