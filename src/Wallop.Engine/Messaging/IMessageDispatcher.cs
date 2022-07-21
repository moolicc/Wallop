using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Messaging
{
    public interface IMessageDispatcher
    {
        void Dispatch(Messenger messenger);
    }
}
