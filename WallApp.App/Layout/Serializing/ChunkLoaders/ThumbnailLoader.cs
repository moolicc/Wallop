using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.App.Layout.Serializing.ChunkLoaders
{
    class ThumbnailLoader : IChunkLoader
    {
        public byte HandledType => ChunkInfo.CHUNK_THUMBNAIL;

        public void LoadChunk(ref ChunkInfo chunk, LayoutReader reader, byte[] buffer, LayoutInfo layout)
        {
            string filepath = App.CreateTempFile();
            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Write))
            {
                stream.Write(buffer, 0, buffer.Length);
            }
            layout.Thumbnail = filepath;
        }
    }
}
