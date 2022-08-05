using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Messaging;

namespace Wallop.Types.Plugins
{
    public class EndPointBase
    {
        public Messenger Messages { get; private set; }

        public EndPointBase(Messenger messages)
        {
            Messages = messages;
        }
    }
}
