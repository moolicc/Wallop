using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Bridge.Data
{
    public class SettingsPayload : IPayload
    {
        public int LayerId { get; set; }
        public bool Enabled { get; set; }
        public float Rotation { get; set; }
        public string Effect { get; set; }
        public (byte R, byte G, byte B) Tint { get; set; }
        public float Opacity { get; set; }

        public SettingsPayload(int layerId, bool enabled, float rotation, string effect, (byte, byte, byte) tint, float opacity)
        {
            LayerId = layerId;
            Enabled = enabled;
            Rotation = rotation;
            Effect = effect;
            Tint = tint;
            Opacity = opacity;
        }
    }
}
