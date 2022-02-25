using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Modules;
using Wallop.DSLExtension.Scripting;
using Wallop.Engine.SceneManagement;
using Wallop.Engine.Scripting.ECS;
using Wallop.Engine.Settings;

namespace Wallop.Engine.Scripting
{
    internal class ScriptedSceneLoader
    {
        private StoredScene _sceneSettings;
        private IEnumerable<Module> _loadedModules;

        // TODO: Populate this from plugins.
        private DSLExtension.Modules.SettingTypes.TypeCache _typeCache;

        public ScriptedSceneLoader(StoredScene settings)
        {
            _sceneSettings = settings;
            _typeCache = new DSLExtension.Modules.SettingTypes.TypeCache();
        }

        // TODO: Actors should be able to have additional settings that are not required that the user can add.
        public Scene LoadFromPackages(string baseDir)
        {
            EngineLog.For<ScriptedSceneLoader>().Info("Loading packages from base directory {dir}...", baseDir);

            // Load all the modules across all packages.
            var packages = PackageLoader.LoadPackages(baseDir);
            _loadedModules = ResolveModules(packages);

            // Create the scene and layouts.
            EngineLog.For<ScriptedSceneLoader>().Info("Creating scene elements...", baseDir);
            var scene = new Scene(_sceneSettings.Name);
            CreateLayouts(scene);
            CreateDirectors(scene);

            return scene;
        }

        private IEnumerable<Module> ResolveModules(IEnumerable<Package> packages)
        {
            var results = new List<Module>();
            foreach (var package in packages)
            {
                foreach (var module in package.DeclaredModules)
                {
                    foreach (var setting in module.ModuleSettings)
                    {
                        setting.CachedType = _typeCache.Types[setting.SettingType];
                    }
                    EngineLog.For<ScriptedSceneLoader>().Debug("Resolving module {module}...", module.ModuleInfo);
                    results.Add(module);
                }
            }
            return results;
        }

        private void CreateLayouts(Scene sceneInstance)
        {
            EngineLog.For<ScriptedSceneLoader>().Info("Creating {numLayouts} layout elements...", _sceneSettings.Layouts.Count);
            
            // Iterate over each layout defined in our loaded settings.
            foreach (var layoutSpecified in _sceneSettings.Layouts)
            {
                // Create the layout which will go in our Scene Tree.
                var layout = new Layout();
                layout.PresentationSize = new System.Numerics.Vector2(800, 600);
                layout.RenderSize = new System.Numerics.Vector2(800, 600);

                // Try to set this layout as the scene's active layout.
                if(layoutSpecified.Active)
                {
                    EngineLog.For<ScriptedSceneLoader>().Info("Setting current layout element ({layout}) as active...", layoutSpecified.Name);
                    if (sceneInstance.ActiveLayout != null)
                    {
                        EngineLog.For<ScriptedSceneLoader>().Warn("The scene already has an active layout ({activeLayout}). The active layout will be changed to ({layout}).", sceneInstance.ActiveLayout.Name, layoutSpecified.Name);
                    }
                    sceneInstance.ActiveLayout = layout;
                }

                sceneInstance.Layouts.Add(layout);

                // Load the actors associated with the current layout along with their modules.
                LoadLayoutActors(layout, layoutSpecified);
            }
        }

        private void LoadLayoutActors(Layout layoutInstance, StoredLayout layoutDefinition)
        {
            EngineLog.For<ScriptedSceneLoader>().Info("Found {actorModules} actor definitions in layout.", layoutDefinition.ActorModules.Count);
            int loaded = 0;
            // Iterate over each actor we're supposed to load for this layout.
            foreach (var actorDefinition in layoutDefinition.ActorModules)
            {
                // Find the module that handles this actor.
                var associatedModule = _loadedModules.FirstOrDefault(
                    m => m.ModuleInfo.ScriptType == ModuleTypes.Actor
                    && m.ModuleInfo.Id == actorDefinition.ModuleId);

                if(associatedModule == null)
                {
                    EngineLog.For<ScriptedSceneLoader>().Error("Module {module} for actor definition {actor} on layout {layout} not found!", actorDefinition.ModuleId, actorDefinition.InstanceName, layoutInstance.Name);
                    continue;
                }
                EngineLog.For<ScriptedSceneLoader>().Debug("Adding actor {actor} to ECS for layout {layout} based on module {module}. ({loaded}/{total})", actorDefinition.InstanceName, layoutInstance.Name, associatedModule.ModuleInfo.Id, loaded + 1, layoutDefinition.ActorModules.Count);
                var actor = new ScriptedActor(associatedModule, actorDefinition, layoutInstance);
                layoutInstance.EcsRoot.AddActor(actor);
                loaded++;
            }

            EngineLog.For<ScriptedSceneLoader>().Info("Loaded {loaded} actors for the current layout.", loaded);
        }

        private void CreateDirectors(Scene sceneInstance)
        {
            EngineLog.For<ScriptedSceneLoader>().Info("Creating {numDirectors} director elements...", _sceneSettings.DirectorModules.Count);

            int loaded = 0;
            foreach (var directorSpecified in _sceneSettings.DirectorModules)
            {
                var directorModule = _loadedModules.FirstOrDefault(
                    m => m.ModuleInfo.ScriptType == ModuleTypes.Director
                    && m.ModuleInfo.Id == directorSpecified.ModuleId);

                if(directorModule == null)
                {
                    EngineLog.For<ScriptedSceneLoader>().Error("Module {module} for director definition {director} not found!", directorSpecified.ModuleId, directorSpecified.InstanceName);
                    continue;
                }

                EngineLog.For<ScriptedSceneLoader>().Debug("Adding director {director} to ECS based on module {module}. ({loaded}/{total})", directorSpecified.InstanceName, directorSpecified.ModuleId, loaded + 1, _sceneSettings.DirectorModules.Count);
                var director = new ScriptedDirector(directorModule, directorSpecified);
                sceneInstance.Directors.Add(director);
                loaded++;
            }

            EngineLog.For<ScriptedSceneLoader>().Info("Loaded {loaded} director elements into the scene.", loaded);
        }
    }
}
