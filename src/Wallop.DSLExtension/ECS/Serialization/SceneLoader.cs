using Wallop.Shared.Modules;

namespace Wallop.Shared.ECS.Serialization
{
    public delegate TScene SceneFactory<TScene>(string sceneName) where TScene : IScene;
    public delegate ILayout LayoutFactory<TScene>(TScene owner, StoredLayout layoutSettings) where TScene : IScene;
    public delegate IActor ActorFactory(ILayout owner, StoredModule actorSettings);
    public delegate IDirector DirectorFactory<TScene>(TScene owner, StoredModule actorSettings) where TScene : IScene;


    /// <summary>
    /// Parses a <see cref="Scene"/> from a loaded in-memory representation contained in an instance of a <see cref="StoredScene" />.
    /// </summary>
    public class SceneLoader
    {
        private StoredScene _sceneSettings;
        private PackageCache _packageCache;

        public SceneLoader(StoredScene settings, PackageCache packageCache)
        {
            _sceneSettings = settings;
            _packageCache = packageCache;
        }

        // TODO: Actors should be able to have additional settings that are not required that the user can add.
        public TScene LoadScene<TScene>(SceneFactory<TScene> sceneFactory, LayoutFactory<TScene> layoutFactory, ActorFactory actorFactory, DirectorFactory<TScene> directorFactory) where TScene : IScene
        {
            // Create the scene and layouts.
            var scene = sceneFactory(_sceneSettings.Name);
            CreateLayouts(scene, layoutFactory, actorFactory);
            CreateDirectors(scene, directorFactory);

            return scene;
        }

        private void CreateLayouts<TScene>(TScene sceneInstance, LayoutFactory<TScene> factory, ActorFactory actorFactory) where TScene : IScene
        {
            // Iterate over each layout defined in our loaded settings.
            foreach (var layoutSpecified in _sceneSettings.Layouts)
            {
                var layout = factory(sceneInstance, layoutSpecified);
                sceneInstance.Layouts.Add(layout);


                // Load the actors associated with the current layout along with their modules.
                LoadLayoutActors(layout, layoutSpecified, actorFactory);
            }
        }

        public void LoadLayoutActors(ILayout layoutInstance, StoredLayout layoutDefinition, ActorFactory factory)
        {
            foreach (var actorDefinition in layoutDefinition.ActorModules)
            {
                var actor = factory(layoutInstance, actorDefinition);
                layoutInstance.EntityRoot.AddActor(actor);
            }
        }

        public void CreateDirectors<TScene>(TScene sceneInstance, DirectorFactory<TScene> factory) where TScene : IScene
        {
            foreach (var item in _sceneSettings.DirectorModules)
            {
                var director = factory(sceneInstance, item);
                sceneInstance.Directors.Add(director);
            }
        }
    }
}
