﻿using System;
using System.Text;

namespace Wallop.Composer.Layout.Serializing.ChunkLoaders
{
    class VarTableLoader : IChunkLoader
    {
        public byte HandledType => throw new NotImplementedException();

        public void LoadChunk(ref ChunkInfo chunk, LayoutReader reader, byte[] buffer, LayoutInfo layout)
        {
            string json = Encoding.Unicode.GetString(buffer);
            var table = Newtonsoft.Json.JsonConvert.DeserializeObject<VarTable>(json);
            layout.VariableTables.Add(table.Layer, table);
        }
    }
}
