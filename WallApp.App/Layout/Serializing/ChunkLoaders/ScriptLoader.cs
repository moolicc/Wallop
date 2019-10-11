using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.App.Layout.Serializing.ChunkLoaders
{
    class ScriptLoader : IChunkLoader
    {
        public byte HandledType => ChunkInfo.CHUNK_SCRIPT;

        public void LoadChunk(ref ChunkInfo chunk, LayoutReader reader, byte[] buffer, LayoutInfo layout)
        {
            layout.Script = Encoding.Unicode.GetString(buffer);
        }
    }
}
