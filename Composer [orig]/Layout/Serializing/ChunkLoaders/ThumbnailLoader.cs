using System.IO;

namespace Wallop.Composer.Layout.Serializing.ChunkLoaders
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
