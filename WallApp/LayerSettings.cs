using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace WallApp
{
    public class LayerSettings
    {
        public Dictionary<string, string> CustomSettings { get; private set; }

        public int LayerId { get; set; }
        public string Module { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public int Rotation { get; set; }
        
        public Color TintColor { get; set; }

        public float Opacity { get; set; }

        public LayerDimensions Dimensions { get; set; }

        public LayerSettings()
        {
            LayerId = -1;
            Module = "";
            Name = "";
            TintColor = Color.White;
            Rotation = 0;
            Opacity = 1.0F;
            Dimensions = new LayerDimensions();
            Enabled = true;
            CustomSettings = new Dictionary<string, string>();
        }

        public string this[string key]
        {
            get
            {
                if (CustomSettings.ContainsKey(key))
                {
                    return CustomSettings[key];
                }
                return "";
            }
            set
            {
                if (CustomSettings.ContainsKey(key))
                {
                    CustomSettings[key] = value;
                }
                else
                {
                    CustomSettings.Add(key, value);
                }
            }
        }
    }
}
