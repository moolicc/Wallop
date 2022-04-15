using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Messaging
{
    public interface IMessageDispatcher
    {
        void Dispatch(Messenger messenger);
    }
}
