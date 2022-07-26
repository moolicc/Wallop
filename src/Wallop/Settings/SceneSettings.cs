using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Scripting;

namespace Wallop.Settings
{
    public class SceneSettings : Cog.Settings
    {
        public string PackageSearchDirectory { get; set; } = @"C:\Users\joel\source\repos\Wallop\modules\";

        /// <summary>
        /// The name of the constant, empty scene.
        /// </summary>
        public string DefaultSceneName { get; set; } = "__Default__";
        /// <summary>
        /// The scene to make active on load.
        /// </summary>
        public string SelectedScene { get; set; } = "__Default__";

        public List<string> ScenePreloadList { get; set; } = new List<string>();
        public ThreadingPolicy UpdateThreadingPolicy { get; set; } = ThreadingPolicy.SingleThread;
        public ThreadingPolicy DrawThreadingPolicy { get; set; } = ThreadingPolicy.SingleThread;

        public SceneSettings Clone()
        {
            return new SceneSettings()
            {
                PackageSearchDirectory = PackageSearchDirectory,
                DefaultSceneName = DefaultSceneName,
                SelectedScene = SelectedScene,
                ScenePreloadList = ScenePreloadList,
                UpdateThreadingPolicy = UpdateThreadingPolicy,
                DrawThreadingPolicy = DrawThreadingPolicy,
            };
        }
    }
}
