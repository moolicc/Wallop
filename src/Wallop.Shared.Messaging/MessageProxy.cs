using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Messaging
{
    public readonly record struct MessageProxy<T>(T Payload, uint MessageId);
}
