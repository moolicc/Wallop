using System;
using System.Collections.Generic;
using System.Text;

namespace WallApp.Bridge.Data
{
    public abstract class PositionPayload : IPayload
    {
        public int LayerId { get; protected set; }
        public int X { get; protected set; }
        public int Y { get; protected set; }
        public int Z { get; protected set; }
        public int W { get; protected set; }

        protected PositionPayload(int layerId, int x, int y, int z, int w)
        {
            LayerId = layerId;
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public void Deconstruct(out int x, out int y, out int z, out int w)
        {
            x = X;
            y = Y;
            z = Z;
            w = W;
        }

        public override string ToString()
        {
            return $"{nameof(PositionPayload)} : [{nameof(LayerId)} {LayerId}] ({X}, {Y}, {Z}, {W})";
        }
    }
}
