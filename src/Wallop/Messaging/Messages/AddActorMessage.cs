using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Messaging.Messages
{
    public readonly record struct AddActorMessage(string ActorName, string? Scene, string? Layout, string BasedOnModule, IEnumerable<KeyValuePair<string, string>>? ModuleSettings = null);
}
