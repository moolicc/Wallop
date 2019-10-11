using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.App.Layout.Serializing.ChunkLoaders
{
    class TitleLoader : IChunkLoader
    {
        public byte HandledType => ChunkInfo.CHUNK_TITLE;

        public void LoadChunk(ref ChunkInfo chunk, LayoutReader reader, byte[] buffer, LayoutInfo layout)
        {
            layout.Title = Encoding.Unicode.GetString(buffer);
        }
    }
}
