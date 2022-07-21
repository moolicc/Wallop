using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Messaging.Messages
{
    // TODO: Scene cloning
    public readonly record struct CreateSceneMessage(string NewSceneName, string BasedOnScene);
}
