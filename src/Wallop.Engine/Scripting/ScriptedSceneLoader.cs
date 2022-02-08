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
        private SceneSettings _sceneSettings;
        private IEnumerable<Module> _loadedModules;

        private DSLExtension.Modules.SettingTypes.TypeCache _typeCache;

        public ScriptedSceneLoader(SceneSettings settings)
        {
            _sceneSettings = settings;
            _typeCache = new DSLExtension.Modules.SettingTypes.TypeCache();
        }

        // TODO: Actors should be able to have additional settings that are not required that the user can add.
        public Scene LoadFromPackages(string baseDir)
        {
            // Load all the modules across all packages.
            var packages = PackageLoader.LoadPackages(baseDir);
            _loadedModules = ResolveModules(packages);

            // Create the scene and layouts.
            var scene = new Scene();
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
                    results.Add(module);
                }
            }
            return results;
        }

        private void CreateLayouts(Scene sceneInstance)
        {
            // Iterate over each layout defined in our loaded settings.
            foreach (var layoutSpecified in _sceneSettings.Layouts)
            {
                // Create the layout which will go in our Scene Tree.
                var layout = new Layout();
                layout.ActualSize = new System.Numerics.Vector2(800, 600);
                layout.RenderSize = new System.Numerics.Vector2(800, 600);

                // Try to set this layout as the scene's active layout.
                if(layoutSpecified.Active)
                {
                    if(sceneInstance.ActiveLayout != null)
                    {
                        // Cannot have more than one active layout.
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
            // Iterate over each actor we're supposed to load for this layout.
            foreach (var actorDefinition in layoutDefinition.ActorModules)
            {
                // Find the module that handles this actor.
                var associatedModule = _loadedModules.FirstOrDefault(
                    m => m.ModuleInfo.ScriptType == ModuleTypes.Actor
                    && m.ModuleInfo.Id == actorDefinition.ModuleId);

                if(associatedModule == null)
                {
                    // TODO: Error.
                    continue;
                }
                var actor = new ScriptedActor(associatedModule, actorDefinition, layoutInstance);
                layoutInstance.EcsRoot.AddActor(actor);
            }
        }

        private void CreateDirectors(Scene sceneInstance)
        {
            foreach (var directorSpecified in _sceneSettings.DirectorModules)
            {
                var directorModule = _loadedModules.FirstOrDefault(
                    m => m.ModuleInfo.ScriptType == ModuleTypes.Director
                    && m.ModuleInfo.Id == directorSpecified.ModuleId);

                if(directorModule == null)
                {
                    // TODO: Error.
                    continue;
                }

                var director = new ScriptedDirector(directorModule, directorSpecified);
                sceneInstance.Directors.Add(director);
            }
        }
    }
}
