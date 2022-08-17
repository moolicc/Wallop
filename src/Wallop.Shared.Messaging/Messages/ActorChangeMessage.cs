using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Messaging.Messages
{
    public readonly record struct ActorChangeMessage(string ActorName, string ActorLayout, string? Scene = null, string? NewName = null, string? NewLayout = null, IEnumerable<KeyValuePair<string, string?>>? ModuleSettings = null);
}
