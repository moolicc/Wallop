using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Messaging.Messages
{
    public readonly record struct AddDirectorMessage(string DirectorName, string BasedOnModule, string Scene, IEnumerable<KeyValuePair<string, string>>? ModuleSettings = null);
}
