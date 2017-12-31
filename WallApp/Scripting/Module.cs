﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WallApp.Scripting
{
    public abstract class Module
    {
        public string File { get; private set; }
        public string SourceFile { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public int MinWidth { get; private set; }
        public int MinHeight { get; private set; }
        public int MaxWidth { get; private set; }
        public int MaxHeight { get; private set; }

        public bool AllowsCustomEffect { get; private set; }


        internal virtual void Init(string file, string sourceFile, string name, string description, int minWidth, int minHeight, int maxWidth, int maxHeight, bool allowsCustomEffects)
        {
            File = file;
            SourceFile = sourceFile;
            Name = name;
            Description = description;

            MinWidth = minWidth;
            MinHeight = minHeight;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;

            AllowsCustomEffect = allowsCustomEffects;
        }

        public abstract SettingsController CreateSettingsController();
        public abstract string GetName();
        public abstract string GetDescription();
        public abstract Controller CreateController();
    }
}
