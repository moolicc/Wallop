using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Messaging.Messages.Json
{
    public record JsonMessages(List<JsonMessage> Messages);

    public record JsonMessage(MessageTypes MessageType, System.Text.Json.Nodes.JsonObject MessageData);

}
