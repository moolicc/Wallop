using System.Text;

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
