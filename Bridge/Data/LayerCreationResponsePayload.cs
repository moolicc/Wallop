using System;
using System.Collections.Generic;
using System.Text;

namespace WallApp.Bridge.Data
{
    public class LayerCreationResponsePayload : IPayload
    {
        public int LayerId { get; private set; }
    }
}
