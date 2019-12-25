using System;
using System.Collections.Generic;
using System.Text;

namespace WallApp.Bridge.Data
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
