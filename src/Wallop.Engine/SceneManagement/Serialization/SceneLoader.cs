using Wallop.DSLExtension.Modules;
using Wallop.Engine.Scripting;
using Wallop.Engine.Scripting.ECS;

namespace Wallop.Engine.SceneManagement.Serialization
{
    /// <summary>
    /// Parses a <see cref="Scene"/> from a loaded in-memory representation contained in an instance of a <see cref="StoredScene" />.
    /// </summary>
    internal class SceneLoader
    {
        private StoredScene _sceneSettings;
        private PackageCache _packageCache;

        public SceneLoader(StoredScene settings, PackageCache packageCache)
        {
            _sceneSettings = settings;
            _packageCache = packageCache;
        }

        // TODO: Actors should be able to have additional settings that are not required that the user can add.
        public Scene LoadScene()
        {
            // Create the scene and layouts.
            EngineLog.For<SceneLoader>().Info("Creating scene elements...");
            var scene = new Scene(_sceneSettings.Name);
            CreateLayouts(scene);
            CreateDirectors(scene);

            return scene;
        }

        private void CreateLayouts(Scene sceneInstance)
        {
            EngineLog.For<SceneLoader>().Info("Creating {numLayouts} layout elements...", _sceneSettings.Layouts.Count);
            
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
                    EngineLog.For<SceneLoader>().Info("Setting current layout element ({layout}) as active...", layoutSpecified.Name);
                    if (sceneInstance.ActiveLayout != null)
                    {
                        EngineLog.For<SceneLoader>().Warn("The scene already has an active layout ({activeLayout}). The active layout will be changed to ({layout}).", sceneInstance.ActiveLayout.Name, layoutSpecified.Name);
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
            EngineLog.For<SceneLoader>().Info("Found {actorModules} actor definitions in layout.", layoutDefinition.ActorModules.Count);

            int loaded = 0;
            // Iterate over each actor we're supposed to load for this layout.
            foreach (var actorDefinition in layoutDefinition.ActorModules)
            {
                var actor = Scripting.ECS.Serialization.ElementLoader.Instance.Load<ScriptedActor>(actorDefinition);
                layoutInstance.EcsRoot.AddActor(actor);
                actor.AddedToLayout(layoutInstance);
                loaded++;
            }

            EngineLog.For<SceneLoader>().Info("Loaded {loaded} actors for the current layout.", loaded);
        }

        private void CreateDirectors(Scene sceneInstance)
        {
            EngineLog.For<SceneLoader>().Info("Creating {numDirectors} director elements...", _sceneSettings.DirectorModules.Count);

            int loaded = 0;
            foreach (var directorSpecified in _sceneSettings.DirectorModules)
            {
                var director = Scripting.ECS.Serialization.ElementLoader.Instance.Load<ScriptedDirector>(directorSpecified);
                sceneInstance.Directors.Add(director);
                loaded++;
            }

            EngineLog.For<SceneLoader>().Info("Loaded {loaded} director elements into the scene.", loaded);
        }
    }
}
