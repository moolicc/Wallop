using System;
using System.Collections.Generic;
using System.Text;

namespace WallApp.Bridge.Data
{
    public class SettingsPayload : IPayload
    {
        public int LayerId { get; set; }
        public bool Enabled { get; set; }
        public float Rotation { get; set; }
        public string Effect { get; set; }
        public (byte, byte, byte) Tint { get; set; }

        public SettingsPayload(int layerId, bool enabled, float rotation, string effect, (byte, byte, byte) tint)
        {
            LayerId = layerId;
            Enabled = enabled;
            Rotation = rotation;
            Effect = effect;
            Tint = tint;
        }
    }
}
