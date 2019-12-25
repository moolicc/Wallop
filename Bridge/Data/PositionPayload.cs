namespace WallApp.Bridge.Data
{
    public abstract class PositionPayload : IPayload
    {
        public int LayerId { get; protected set; }
        public float X { get; protected set; }
        public float Y { get; protected set; }
        public float Z { get; protected set; }
        public float W { get; protected set; }

        public bool UseAbsolutes { get; protected set; }
        public bool UseMargins { get; protected set; }

        protected PositionPayload(int layerId, float x, float y, float z, float w, bool useAbsolutes, bool useMargins)
        {
            LayerId = layerId;
            X = x;
            Y = y;
            Z = z;
            W = w;
            UseAbsolutes = useAbsolutes;
            UseMargins = useMargins;
        }

        public void Deconstruct(out float x, out float y, out float z, out float w)
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
