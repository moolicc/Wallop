﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace WallApp.Engine
{
    public class LayerSettings : ICloneable
    {
        public Dictionary<string, string> CustomSettings { get; private set; }

        public int LayerId { get; private set; }
        public string Module { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public int Rotation { get; set; }

        public string Effect { get; set; }
        public Color TintColor { get; set; }

        public float Opacity { get; set; }

        public LayerDimensions Dimensions { get; set; }

        public LayerSettings(int layerId, string module)
        {
            LayerId = layerId;
            Module = module;
            Name = "";
            TintColor = Color.White;
            Rotation = 0;
            Opacity = 1.0F;
            Dimensions = new LayerDimensions();
            Enabled = true;
            Effect = "";
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

        public object Clone()
        {
            return new LayerSettings(LayerId, Module)
            {
                CustomSettings = this.CustomSettings,
                Description = this.Description,
                Dimensions = (LayerDimensions)this.Dimensions.Clone(),
                Enabled = this.Enabled,
                Name = this.Name,
                Opacity = this.Opacity,
                Rotation = this.Rotation,
                TintColor = this.TintColor,
            };
        }
    }
}
