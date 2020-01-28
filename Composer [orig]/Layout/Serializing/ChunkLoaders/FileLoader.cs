namespace Wallop.Composer.Layout.Serializing.ChunkLoaders
{
    class FileLoader : IChunkLoader
    {
        public byte HandledType => ChunkInfo.CHUNK_INCLUDED_FILE;

        public void LoadChunk(ref ChunkInfo chunk, LayoutReader reader, byte[] buffer, LayoutInfo layout)
        {
            layout.Resources.Add(reader.StringTable[chunk.TagIndex], buffer);
        }
    }
}
