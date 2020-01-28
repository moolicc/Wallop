namespace Wallop.Composer.Layout.Serializing
{
    interface IChunkLoader
    {
        byte HandledType { get; }

        void LoadChunk(ref ChunkInfo chunk, LayoutReader reader, byte[] buffer, LayoutInfo layout);
    }
}
