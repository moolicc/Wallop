﻿using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.ECS;
using Wallop.Shared.Messaging;
using Wallop.Shared.Messaging.Messages;
using Wallop.SceneManagement.Serialization;
using Wallop.Scripting;
using Wallop.Scripting.ECS;
using Wallop.Settings;
using Wallop.Shared.Scripting;
using Wallop.Shared.ECS;
using Wallop.Shared.ECS.Serialization;
using Wallop.Shared.Modules;
using Wallop.Types;

namespace Wallop.Handlers
{

    internal class SceneHandler : EngineHandler
    {
        // TODO: SaveConfigMessage
        // TODO: LoadConfigMessage
        

        public PackageCache PackageCache { get; private set; }
        public SceneSettings SceneSettings => _sceneSettings;
        public Scene ActiveScene => _activeScene;

        internal SceneStore SceneStore => _sceneStore;

        private TaskHandler _taskHandler;

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
            SubscribeToEngineMessages<LayoutChangeMessage>(HandleChangeLayout);
            SubscribeToEngineMessages<SetActiveLayoutMessage>(HandleSetActiveLayout);
            SubscribeToEngineMessages<AddActorMessage>(HandleAddActor);
            SubscribeToEngineMessages<ActorChangeMessage>(HandleChangeActor);
            SubscribeToEngineMessages<CreateSceneMessage>(HandleCreateScene);
            SubscribeToEngineMessages<SceneSaveMessage>(HandleSceneSave);
            SubscribeToEngineMessages<GetSceneMessage>(HandleGetScene);

            SubscribeToEngineMessages<AddDirectorMessage>(HandleAddDirector);
            SubscribeToEngineMessages<ReloadModuleMessage>(HandleReloadModule);
        }

        public override Command? GetCommandLineCommand()
        {
            var importLocationOpts = new Argument<string>("location", "Specifies a scene configuration file to import");

            var importAsNameOpts = new Option<string>(
                new[] { "--name", "-n" },
                () => "",
                "Specifies a name to apply to the imported scene");


            var reloadModuleIdOpts = new Argument<string>("moduleid", "The ID of the module to reload");
            var reloadModuleKeepStateOpts = new Option<bool>(
                new[] { "-s", "--keep-state" },
                () => false,
                "Specifies whether or not to retain the state of any actors using the module");





            // EngineApp.exe scene load ...
            var sceneLoadCommand = new Command("load", "Loads a scene configuration")
            {
                importLocationOpts,
                importAsNameOpts,
            };

            sceneLoadCommand.SetHandler(
                (string importLocation, string importName) =>
                {
                    App.Messenger.Put(new SceneChangeMessage(importLocation));
                }, importLocationOpts, importAsNameOpts);


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
                sceneLoadCommand,
                sceneReloadCommand,
                sceneInfoCommand,
            };


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
            _taskHandler = new TaskHandler(_sceneSettings.UpdateThreadingPolicy, _sceneSettings.DrawThreadingPolicy);
        }

        private void SetupDefaultScene()
        {
            EngineLog.For<SceneHandler>().Info("Setting up default scene...");

            var defaultSettings = new StoredScene()
            {
                Name = _sceneSettings.DefaultSceneName,
                DirectorModules = new List<StoredModule>()
                {
                },
                Layouts = new List<StoredLayout>()
                {
                    new StoredLayout()
                    {
                        Name = "layout1",
                        Active = true,
                        ScreenIndex = 1,
                        ActorModules = new List<StoredModule>()
                        {
                            new StoredModule()
                            {
                                InstanceName = "actor1",
                                ModuleId = "Image1.0",
                                Settings = new List<StoredSetting>()
                                {
                                    new StoredSetting("file", "C:\\test.png"),
                                    new StoredSetting("X", "1.0"),
                                    new StoredSetting("Y", "1.0"),
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

            var bindableComponentTypes = App.GetService<BindableComponentTypeCache>().OrThrow();
            //var taskProvider = App.GetService<TaskHandlerProvider>().OrThrow();
            var pluginContext = App.GetService<PluginPantry.PluginContext>().OrThrow();
            var hostFunctions = App.GetService<ScriptHostFunctions>().OrThrow();
            var engineProviders = App.GetService<ScriptEngineProviderCache>().OrThrow();


            EngineLog.For<SceneHandler>().Info("Initializing ECS Element invokers...");
            var elementLoader = new Scripting.ECS.Serialization.ElementLoader(PackageCache);
            var elementInitializer = new Scripting.ECS.Serialization.ElementInitializer(App, engineProviders, _taskHandler, hostFunctions, pluginContext, bindableComponentTypes);

            EngineLog.For<SceneHandler>().Info("Constructing scene...");
            var sceneLoader = new SceneLoader(settings, PackageCache);
            var scene = sceneLoader.LoadScene(n => new Scene(n), LoadLayout, LoadActor, LoadDirector);

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
            var sceneSaver = new SceneSaver(keepState ? SettingsSaveOptions.EntireState : SettingsSaveOptions.RequiredSettings, PackageCache);

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
            var savedLayouts = new List<(ILayout SceneLayout, StoredLayout SavedInfo)>();

            int actorCount = 0;
            foreach (var layout in ActiveScene.Layouts)
            {
                var applicableActors = layout.EntityRoot.GetActors()
                    .Where(a => a is ScriptedActor xA && xA.ModuleDeclaration.ModuleInfo.Id == moduleId);

                var savedActors = sceneSaver.SaveActors(applicableActors.Select(a => (ScriptedActor)a));

                var storedLayout = new StoredLayout();
                storedLayout.ActorModules = savedActors.ToList();

                // Remove the actors from the layout.
                foreach (var item in applicableActors)
                {
                    layout.EntityRoot.Remove(item);
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
            sceneLoader.CreateDirectors(ActiveScene, LoadDirector);

            EngineLog.For<SceneHandler>().Info("Reloading {actorCount} actors within {layoutCount} layouts with module '{module}'...", actorCount, savedLayouts.Count, moduleId);
            foreach (var item in savedLayouts)
            {
                foreach (var savedActor in item.SavedInfo.ActorModules)
                {
                    try
                    {
                        SafetyNet.Handle<SceneHandler, StoredModule, ScriptedActor>((a) => Scripting.ECS.Serialization.ElementLoader.Instance.Load<ScriptedActor>(a), savedActor, out var actor)
                            .Then<ScriptedActor>((a) =>
                            {
                                item.SceneLayout.EntityRoot.AddActor(a);
                                a.AddedToLayout(item.SceneLayout);
                            }).Next<ScriptedActor>(a =>
                            {
                                Scripting.ECS.Serialization.ElementInitializer.Instance.InitializeElement(a, _activeScene);
                            });


                        
                    }
                    catch (Exception ex)
                    {
                        EngineLog.For<SceneHandler>().Error(ex, "Failed to load or initialize actor '{actor}' with module '{module}'!", savedActor.InstanceName, savedActor.ModuleId);
                        throw;
                    }
                }
            }
        }


        private ILayout LoadLayout(Scene scene, StoredLayout stored)
        {
            var layout = new Layout(stored.Name);
            var screens = ScreenInfo.GetScreens();

            layout.Screen = screens[stored.ScreenIndex];
            layout.RenderSize = new System.Numerics.Vector2(stored.RenderWidth, stored.RenderHeight);

            if(stored.Active)
            {
                layout.Activate();
            }
            return layout;
        }

        private IActor LoadActor(ILayout owner, StoredModule stored)
        {
            ScriptedActor? actor = null;

            try
            {
                actor = Scripting.ECS.Serialization.ElementLoader.Instance.Load<ScriptedActor>(stored);
                if (actor == null)
                {
                    throw new NullReferenceException();
                }
                owner.EntityRoot.AddActor(actor);
                actor.AddedToLayout(owner);
                EngineLog.For<SceneLoader>().Info("Actor {actor} loaded with module {module}!", stored.InstanceName, stored.ModuleId);
            }
            catch (Exception ex)
            {
                EngineLog.For<SceneLoader>().Error(ex, "Failed to load or initialize actor {actor} with module {module}!", stored.InstanceName, stored.ModuleId);
            }

            return actor!;
        }

        private IDirector LoadDirector<TScene>(TScene owner, StoredModule stored) where TScene : IScene
        {
            ScriptedDirector? director = null;

            try
            {
                director = Scripting.ECS.Serialization.ElementLoader.Instance.Load<ScriptedDirector>(stored);
                if (director == null)
                {
                    throw new NullReferenceException();
                }
                EngineLog.For<SceneLoader>().Info("Director {director} loaded with module {module}!", stored.InstanceName, stored.ModuleId);
            }
            catch (Exception ex)
            {
                EngineLog.For<SceneLoader>().Error(ex, "Failed to load or initialize director '{director}' with module '{module}'!", stored.InstanceName, stored.ModuleId);
                
            }
            return director!;
        }

        private object HandleSceneSettings(SceneSettingsMessage message, uint messageId)
        {
            // Handled within the below call to SwitchScene()
            //_sceneSettings.SelectedScene = message.Settings.SelectedScene;

            try
            {
                _sceneSettings.DefaultSceneName = message.DefaultSceneName ?? _sceneSettings.DefaultSceneName;

                if (message.PackageSearchDirectory != null && _sceneSettings.PackageSearchDirectory != message.PackageSearchDirectory)
                {
                    _sceneSettings.PackageSearchDirectory = message.PackageSearchDirectory;
                    PackageCache.ReloadAll(message.PackageSearchDirectory);
                }

                if (message.DrawPolicy.HasValue && (int)_sceneSettings.DrawThreadingPolicy != message.DrawPolicy.Value)
                {
                    _sceneSettings.DrawThreadingPolicy = (ThreadingPolicy)message.DrawPolicy.Value;
                    _taskHandler.DrawPolicy = (ThreadingPolicy)message.DrawPolicy.Value;
                }
                if (message.UpdatePolicy.HasValue && (int)_sceneSettings.UpdateThreadingPolicy != message.UpdatePolicy.Value)
                {
                    _sceneSettings.UpdateThreadingPolicy = (ThreadingPolicy)message.UpdatePolicy.Value;
                    _taskHandler.UpdatePolicy = (ThreadingPolicy)message.UpdatePolicy.Value;
                }


                foreach (var item in message.ScenePreloads ?? Array.Empty<string>())
                {
                    if (item == message.SelectedScene || item == message.DefaultSceneName)
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

                if(message.SelectedScene != null && message.SelectedScene != _sceneSettings.SelectedScene)
                {
                    SwitchScene(message.SelectedScene);
                }
            }
            catch (Exception ex)
            {
                return Fail(messageId, ex);
            }

            return Success(messageId, _sceneSettings.Clone());
        }

        private object HandleSceneChange(SceneChangeMessage message, uint messageId)
        {
            try
            {
                SwitchScene(message.NewScene);
            }
            catch (Exception ex)
            {
                return Fail(messageId, ex);
            }
            return Success(messageId);
        }

        private object HandleAddLayout(AddLayoutMessage message, uint messageId)
        {
            // TODO: Handle cloning layouts.

            if (message.TargetScene == null)
            {
                var newLayout = new Layout(message.Name);

                newLayout.Screen = ScreenInfo.GetScreens()[message.ScreenIndex];
                newLayout.RenderSize = new System.Numerics.Vector2(100, 100); //message.RenderSize ?? new System.Numerics.Vector2(newLayout.Screen.Bounds.Z / 2, newLayout.Screen.Bounds.W / 2);
                newLayout.PresentationBounds = message.PresentationBounds ?? newLayout.Screen.Bounds;

                var scene = _activeScene;
                scene.Layouts.Add(newLayout);


                if (message.MakeActive)
                {
                    newLayout.Activate();
                }
            }
            else
            {
                var newLayout = new StoredLayout();
                newLayout.Name = message.Name;
                newLayout.Active = message.MakeActive;

                var scene = _sceneStore.Get(message.TargetScene);
                if (scene == null)
                {
                    return Invalid(messageId, "The target scene does not exist.");
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

            return Success(messageId);
        }

        private object? HandleChangeLayout(LayoutChangeMessage message, uint messageId)
        {
            var layout = _activeScene.Layouts.FirstOrDefault(l => l.Name == message.Layout) as Layout;

            if(layout == null)
            {
                return Invalid(messageId, "Failed to find layout.");
            }

            if(message.NewName != null)
            {
                // TODO:
                //layout.Name = message.NewName;
            }

            if (message.NewActive != null)
            {
                if (message.NewActive.Value && !layout.IsActive)
                {
                    layout.Activate();
                }
                else if (layout.IsActive)
                {
                    layout.Deactivate();
                }
            }

            if(message.NewScreen != null)
            {
                var screens = ScreenInfo.GetScreens();
                layout.Screen = screens[message.NewScreen.Value];
            }

            if(message.NewRenderSize != null)
            {
                layout.RenderSize = message.NewRenderSize.Value;
            }

            if(message.NewPresentationBounds != null)
            {
                layout.PresentationBounds = message.NewPresentationBounds.Value;
            }

            return Success(messageId);
        }

        private object HandleSetActiveLayout(SetActiveLayoutMessage message, uint messageId)
        {
            var layout = _activeScene.Layouts.FirstOrDefault(l => l.Name == message.LayoutName);
            if (layout == null)
            {
                // TODO: error.
                return Invalid(messageId, "The target layout does not exist.");
            }

            layout.Activate();
            return Success(messageId);
        }

        private object HandleAddActor(AddActorMessage message, uint messageId)
        {
            // TODO: Stored bindings

            // Create an actor based on the module specified in the message and add it to the specified layout.
            var actorDefinition = new StoredModule()
            {
                InstanceName = message.ActorName,
                ModuleId = message.BasedOnModule,
                Settings = StoredSetting.FromEnumerable(message.ModuleSettings ?? Array.Empty<KeyValuePair<string, string>>()).ToList(),
            };


            // If there was no specified scene, or the specified scene is active then we add the actor
            // to the current scene.

            // Otherwise, we add it to the specified scene in storage.
            if (message.Scene == null || message.Scene.Equals(_activeScene.Name))
            {
                ScriptedActor? actor = null;

                try
                {
                    actor = Scripting.ECS.Serialization.ElementLoader.Instance.Load<ScriptedActor>(actorDefinition);
                    if (actor == null)
                    {
                        throw new NullReferenceException();
                    }
                }
                catch (Exception ex)
                {
                    EngineLog.For<SceneHandler>().Error(ex, "Failed to load or initialize actor '{actor}' with module '{module}'!", actorDefinition.InstanceName, actorDefinition.ModuleId);
                    return Fail(messageId, ex);
                }

                ILayout? layout = null;
                if (string.IsNullOrEmpty(message.Layout))
                {
                    layout = _activeScene.GetActiveLayouts().First();
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
                    return Invalid(messageId, "The target layout does not exist.");
                }

                layout.EntityRoot.AddActor(actor);
                actor.AddedToLayout(layout);
                Scripting.ECS.Serialization.ElementInitializer.Instance.InitializeElement(actor, _activeScene);
            }
            else
            {
                var scene = _sceneStore.Get(message.Scene);
                if (scene == null)
                {
                    return Invalid(messageId, "The target scene does not exist.");
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
                    return Invalid(messageId, "The target layout does not exist.");
                }
                layout.ActorModules.Add(actorDefinition);
            }

            return Success(messageId);
        }

        private object? HandleChangeActor(ActorChangeMessage message, uint messageId)
        {
            var scene = _activeScene.Name;
            if(message.Scene != null)
            {
                scene = message.Scene;
            }

            var layout = ActiveScene.Layouts.FirstOrDefault(l => l.Name == message.ActorLayout);
            if(layout == null)
            {
                return Invalid(messageId, "Failed to find layout of that name.");
            }

            var actor = layout.EntityRoot.GetActors<ScriptedActor>().FirstOrDefault(a => a.Name == message.ActorName);
            if (actor == null)
            {
                return Invalid(messageId, "Failed to find an actor of that name.");
            }

            var context = actor.GetAttachedScriptContext();

            if (message.ModuleSettings != null)
            {
                foreach (var setting in message.ModuleSettings)
                {
                    var storedSetting = actor.ModuleDeclaration.ModuleSettings.FirstOrDefault(s => s.SettingName == setting.Key);

                    if(storedSetting == null || storedSetting.CachedType == null || setting.Value == null)
                    {
                        context.SetValue(setting.Key, setting.Value);
                    }
                    else
                    {
                        if(storedSetting.CachedType.TryDeserialize(setting.Value, out var value, null))
                        {
                            context.SetValue(setting.Key, value);
                        }
                        else
                        {
                            return Invalid(messageId, "Failed to deserialize setting value.");
                        }
                    }
                }
            }


            return Success(messageId);
        }


        private object HandleCreateScene(CreateSceneMessage message, uint messageId)
        {
            try
            {
                var scene = new StoredScene();
                scene.Name = message.NewSceneName;
                _sceneStore.Add(scene);

                SwitchScene(message.NewSceneName);
            }
            catch (Exception ex)
            {
                return Fail(messageId, ex);
            }
            return Success(messageId);
        }

        private object HandleSceneSave(SceneSaveMessage message, uint messageId)
        {
            try
            {
                SaveCurrentSceneConfig((SettingsSaveOptions)message.Options!, message.Location!);
            }
            catch (Exception ex)
            {
                return Fail(messageId, ex);
            }
            return Success(messageId);
        }

        private object? HandleGetScene(GetSceneMessage message, uint messageId)
        {
            var saver = new SceneSaver(SettingsSaveOptions.OptionalSettings | SettingsSaveOptions.RequiredSettings, PackageCache);
            var stored = saver.Save(_activeScene);

            return Success(messageId, stored);
        }


        private object HandleAddDirector(AddDirectorMessage message, uint messageId)
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
                ScriptedDirector? director = null;

                try
                {
                    director = Scripting.ECS.Serialization.ElementLoader.Instance.Load<ScriptedDirector>(directorDefinition);
                    if (director == null)
                    {
                        throw new NullReferenceException();
                    }
                }
                catch (Exception ex)
                {
                    EngineLog.For<SceneHandler>().Error(ex, "Failed to load or initialize director '{director}' with module '{module}'!", directorDefinition.InstanceName, directorDefinition.ModuleId);
                    return Fail(messageId, ex);
                }


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
                    return Invalid(messageId, "The target scene does not exist.");
                }

                // Add the director definition to the stored scene.
                scene.DirectorModules.Add(directorDefinition);
            }
            return Success(messageId);
        }

        public object HandleReloadModule(ReloadModuleMessage message, uint messageId)
        {
            try
            {
                ReloadModule(message.ModuleId, message.keepState);
            }
            catch (Exception ex)
            {
                return Fail(messageId, ex);
            }
            return Success(messageId);
        }


        public void SceneUpdate()
        {
            _activeScene.Update();
            _taskHandler.OnUpdate();
        }

        public void SceneDraw()
        {
            _activeScene.Draw();
            _taskHandler.OnDraw();
        }

        public override void Shutdown()
        {
            _activeScene.Shutdown();
            EngineLog.For<SceneHandler>().Info("SceneHandler shutdown.");
        }
    }
}
