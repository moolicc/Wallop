using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.App.Layout.Serializing.ChunkLoaders
{
    static class ChunkLoading
    {
        private static Dictionary<byte, IChunkLoader> _resolvers;

        static ChunkLoading()
        {
            _resolvers = new Dictionary<byte, IChunkLoader>();

            var authorLoader = new AuthorLoader();
            var fileLoader = new FileLoader();
            var scriptLoader = new ScriptLoader();
            var thumbnailLoader = new ThumbnailLoader();
            var titleLoader = new TitleLoader();
            var varTableLoader = new VarTableLoader();

            _resolvers.Add(authorLoader.HandledType, authorLoader);
            _resolvers.Add(fileLoader.HandledType, fileLoader);
            _resolvers.Add(scriptLoader.HandledType, scriptLoader);
            _resolvers.Add(thumbnailLoader.HandledType, thumbnailLoader);
            _resolvers.Add(titleLoader.HandledType, titleLoader);
            _resolvers.Add(varTableLoader.HandledType, varTableLoader);
        }

        public static void Resolve(ref ChunkInfo chunkInfo, LayoutReader reader, byte[] buffer, LayoutInfo layout)
        {
            if(_resolvers.TryGetValue(chunkInfo.ChunkType, out var resolver))
            {
                resolver.LoadChunk(ref chunkInfo, reader, buffer, layout);
            }
            else
            {
                //TODO: Handle default case where there is no known chunk loader.
            }
        }
    }
}
