using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Types.Plugin;

namespace StandardComponents
{
    public class Plugin
    {
        [PluginPantry.Extending.PluginEntryPoint("StandardComponents", "1.0.0.0")]
        public void Startup(PluginPantry.Extending.PluginInformation pluginInfo)
        {
            pluginInfo.Exposed.RegisterEndPoint<IBindableTypeRegistrationEndPoint>(nameof(RegisterComponents), this, pluginInfo.PluginId);
        }

        public void RegisterComponents(IBindableTypeRegistrationEndPoint ctx)
        {
            ctx.Bindable<PositionComponent>(nameof(PositionComponent));
        }
    }
}
