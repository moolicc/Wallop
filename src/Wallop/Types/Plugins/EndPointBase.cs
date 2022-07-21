using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Messaging;
namespace Wallop.Types.Plugins
{
    public class EndPointBase
    {
        public Messaging.Messenger Messages { get; private set; }

        public EndPointBase(Messenger messages)
        {
            Messages = messages;
        }
    }
}
