using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Messaging.Json
{
    public record JsonMessages(List<JsonMessage> Messages);

    public record JsonMessage(string MessageType, System.Text.Json.Nodes.JsonObject MessageData);

}
