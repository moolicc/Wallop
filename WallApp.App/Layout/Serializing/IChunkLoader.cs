using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.App.Layout.Serializing
{
    interface IChunkLoader
    {
        byte HandledType { get; }

        void LoadChunk(ref ChunkInfo chunk, LayoutReader reader, byte[] buffer, LayoutInfo layout);
    }
}
