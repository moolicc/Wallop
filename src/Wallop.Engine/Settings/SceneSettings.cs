using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.Scripting;

namespace Wallop.Engine.Settings
{
    public class SceneSettings : Cog.Settings
    {
        public string PackageSearchDirectory { get; set; } = @"C:\Users\joel\source\repos\moolicc\Wallop\modules\squaretest";
        public string DefaultSceneName { get; set; } = "__Default__";
        public string SelectedScene { get; set; } = "__Default__";
        public List<string> ScenePreloadList { get; set; } = new List<string>();
        public ThreadingPolicy UpdateThreadingPolicy { get; set; } = ThreadingPolicy.SingleThread;
        public ThreadingPolicy DrawThreadingPolicy { get; set; } = ThreadingPolicy.SingleThread;
    }
}
