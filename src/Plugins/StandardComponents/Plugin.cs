using PluginPantry;
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
        [EntryPoint("name", "StandardComponents", "version", "1.0.0.0")]
        public void Startup(PluginContext context, Guid id)
        {
            context.RegisterAction<IBindableTypeRegistrationEndPoint, Plugin>(id, nameof(RegisterComponents), this);
        }

        public void RegisterComponents(IBindableTypeRegistrationEndPoint ctx)
        {
            ctx.Bindable<PositionComponent>(nameof(PositionComponent));
        }
    }
}
