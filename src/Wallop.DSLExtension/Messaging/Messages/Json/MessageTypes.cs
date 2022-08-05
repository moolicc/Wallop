using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Messaging.Messages.Json
{
    public enum MessageTypes
    {
        AddActor,
        AddLayout,
        AddDirector,
        CreateScene,
        ReloadModule,
        ChangeScene,
        SaveScene,
        ActiveLayout,

        SceneSettingsChange,
        GraphicsChange,

    }
}
