using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Bridge.Data
{
    public class LayerDeletionPayload : IPayload
    {
        public int LayerId { get; set; }

        public LayerDeletionPayload(int layerId)
        {
            LayerId = layerId;
        }
    }
}
