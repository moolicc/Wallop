using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Settings
{
    public partial class AppSettings : Cog.Settings
    {

        public DependencyDirectory[] DependencyPaths { get; set; } = new[]
            {
                new DependencyDirectory()
                {
                    Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins"),
                    Recursive = true
                }
            };

        public static string InstanceName { get => instanceName; set => instanceName = value.Replace("\"", ""); }
        private static string instanceName = Program.DEFAULT_NAME;
    }
}
