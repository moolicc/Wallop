﻿using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WallApp.Scripting
{
    public abstract class Controller
    {
        public LayerSettings Settings { get; set; }
        public Module Module { get; internal set; }
        public Rendering Rendering { get; internal set; }
        
        public abstract void Setup();
        public abstract void EnabledChanged();
        public abstract void Dispose();

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);
    }
}
