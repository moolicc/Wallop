using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Messaging.Remoting
{
    internal readonly record struct PullMessage(uint MessageID, Json.JsonMessage? EncodedMessage);
}
