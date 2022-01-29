using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Rendering
{
    public readonly record struct Color
    {
        public static readonly Color Red = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        public static readonly Color DarkRed = new Color(0.5f, 0.0f, 0.0f, 1.0f);
        public static readonly Color Green = new Color(0.0f, 1.0f, 0.0f, 1.0f);
        public static readonly Color DarkGreen = new Color(0.0f, 0.5f, 0.0f, 1.0f);
        public static readonly Color CornFlowerBlue = new Color(0.3921f, 0.5843f, 0.9294f, 1.0f);
        public static readonly Color Blue = new Color(0.0f, 0.0f, 1.0f, 1.0f);
        public static readonly Color DarkBlue = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        public static readonly Color Yellow = new Color(1.0f, 1.0f, 0.0f, 1.0f);
        public static readonly Color Orange = new Color(1.0f, 0.36f, 0.0f, 1.0f);
        public static readonly Color LightGray = new Color(0.70f, 0.70f, 0.70f, 1.0f);
        public static readonly Color Gray = new Color(0.25f, 0.25f, 0.25f, 1.0f);
        public static readonly Color White = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public static readonly Color Black = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        public static readonly Color Transparent = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        public float R { get; init; }
        public float G { get; init; } 
        public float B { get; init; }
        public float A { get; init; }


        public Color(Vector4D<byte> color)
            : this(color.X, color.Y, color.Z, color.W)
        {
        }

        public Color(Vector4D<float> color)
            : this(color.X, color.Y, color.Z, color.W)
        {
        }

        public Color(byte r, byte g, byte b, byte a)
            : this(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f)
        {
        }

        public Color(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static explicit operator Vector4D<float>(Color c)
            => new Vector4D<float>(c.R, c.G, c.B, c.A);
        public static explicit operator Vector4D<byte>(Color c)
            => new Vector4D<byte>((byte)(c.R * 255), (byte)(c.G * 255), (byte)(c.B * 255), (byte)(c.A * 255));
    }
}
