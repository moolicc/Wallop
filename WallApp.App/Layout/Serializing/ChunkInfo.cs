using System.Runtime.InteropServices;

namespace WallApp.App.Layout.Serializing
{
    [StructLayout(LayoutKind.Explicit)]
    struct ChunkInfo
    {
        // 1 - 20 reserved for layout metadata.
        public const byte CHUNK_TITLE = 1;
        public const byte CHUNK_AUTHOR = 2;
        public const byte CHUNK_THUMBNAIL = 3;
        public const byte CHUNK_SCRIPT = 4;

        // Module Info contains information about what modules this layout should care about.
        public const byte CHUNK_MODULE_INFO = 20;

        // Included Module contains an entire module encoded in the file.
        public const byte CHUNK_INCLUDED_MODULE = 21;


        // 100+ is reserved for resources that either a layer or layout script might consume.
        public const byte CHUNK_INCLUDED_FILE = 100;
        public const byte CHUNK_INCLUDED_VAR_TABLE = 101;



        /// <summary>
        /// The length of data this info item takes up.
        /// </summary>
        [FieldOffset(0)]
        public byte BlockLength;

        /// <summary>
        /// An offset from the start of the string table to this chunk's tag.
        /// </summary>
        [FieldOffset(1)]
        public byte TagIndex;

        /// <summary>
        /// The type of chunk that is contained in the payload.
        /// </summary>
        [FieldOffset(2)]
        public byte ChunkType;

        /// <summary>
        /// An offset from the start of the payload to the start of the chunk.
        /// </summary>
        [FieldOffset(3)]
        public long ChunkStartOffset;

        /// <summary>
        /// The length of data the chunk takes up, regardless of compression, in the payload.
        /// </summary>
        [FieldOffset(11)]
        public long ChunkLength;

        /// <summary>
        /// A hash of the chunk to verify integrity.
        /// </summary>
        [FieldOffset(19)]
        public uint ChunkHash;
    }
}
