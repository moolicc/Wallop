using Wallop.Shared.ECS;
using Wallop.Shared.ECS.Serialization;
using Wallop.Scripting;
using Wallop.Scripting.ECS;
using Wallop.Shared.Modules;

namespace Wallop.Ecs.Serialization
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



    }
}
