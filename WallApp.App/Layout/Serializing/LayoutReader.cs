using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.IO.Compression;

namespace WallApp.App.Layout.Serializing
{
    class LayoutReader
    {
        [Flags]
        private enum HeaderFlags : byte
        {
            None = 0x00,
            Compression = 0x01,
            NA0 = 0x02,
            NA1 = 0x04,
            NA2 = 0x08,
            NA3 = 0x0F,
            NA4 = 0xFF,
            NA5 = 0x40,
            NA6 = 0x80,
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct WappHeader
        {
            // File version 1.0
            public const byte EXPECTED_MAJOR = 1;
            public const byte EXPECTED_MINOR = 0;

            // WAPP
            public const int EXPECTED_ID = 0x57_41_50_50;

            /// <summary>
            /// The file format ID.
            /// </summary>
            [FieldOffset(0)]
            public int FileId;

            /// <summary>
            /// File verion major.
            /// </summary>
            [FieldOffset(4)]
            public byte Major;

            /// <summary>
            /// File version minor.
            /// </summary>
            [FieldOffset(5)]
            public byte Minor;

            /// <summary>
            /// Length of this block.
            /// </summary>
            [FieldOffset(6)]
            public byte BlockLength;

            /// <summary>
            /// An offset from the start of the file to the string table start.
            /// </summary>
            [FieldOffset(7)]
            public uint StringTableOffset;

            /// <summary>
            /// An offset from the start of the file to the chunk index start.
            /// </summary>
            [FieldOffset(11)]
            public uint ChunkIndexOffset;

            /// <summary>
            /// An offset from the start of the file to the payload start.
            /// </summary>
            [FieldOffset(15)]
            public uint PayloadOffset;

            /// <summary>
            /// The flags contained in the single-byte.
            /// </summary>
            [FieldOffset(19)]
            public HeaderFlags Flags;

            public bool PayloadCompressed
            {
                get
                {
                    return (HeaderFlags.Compression & Flags) == HeaderFlags.Compression;
                }
                set
                {
                    if (value)
                    {
                        Flags = (Flags | HeaderFlags.Compression);
                    }
                    else
                    {
                        Flags = Flags & ~HeaderFlags.Compression;
                    }
                }
            }
        }



        public Stream DataStream { get; private set; }
        public List<string> StringTable { get; private set; }

        private WappHeader _header;
        private ChunkInfo[] _chunkIndex;


        public LayoutReader(Stream dataStream)
        {
            DataStream = dataStream;
            StringTable = new List<string>();
            ReadHeader();
        }

        public LayoutInfo LoadLayout()
        {
            LayoutInfo result = new LayoutInfo();

            if(DataStream.CanSeek && DataStream.Position != _header.StringTableOffset)
            {
                DataStream.Position = _header.StringTableOffset;
            }
            else
            {
                // Else, let's just hope we're in the right place.
            }

            ReadStringTable();
            ReadChunkIndex();
            ReadChunks(result);

            return result;
        }

        private void ReadHeader()
        {
            // Read the file format ID.
            byte[] buffer = new byte[4];
            DataStream.Read(buffer, 0, 4);
            int fileIdentifier = BitConverter.ToInt32(buffer, 0);

            // Check for valid file.
            if (fileIdentifier != WappHeader.EXPECTED_ID)
            {
                throw new FileFormatException("Invalid input file.");
            }

            // Read the major.
            byte major = (byte)DataStream.ReadByte();

            // Read the minor.
            byte minor = (byte)DataStream.ReadByte();

            // Read the block length.
            byte blockLength = (byte)DataStream.ReadByte();


            // Keep up with how many bytes we've consumed.
            byte readData = 7;


            // Read the string table's index.
            DataStream.Read(buffer, 0, 4);
            uint stringTableOffset = BitConverter.ToUInt32(buffer, 0);
            readData += 4;

            CheckUnexpectedOutOfData(blockLength, readData);


            // Read the chunk index's index.
            DataStream.Read(buffer, 0, 4);
            uint chunkIndexOffset = BitConverter.ToUInt32(buffer, 0);
            readData += 4;

            CheckUnexpectedOutOfData(blockLength, readData);


            // Read the payload's index.
            DataStream.Read(buffer, 0, 4);
            uint payloadOffset = BitConverter.ToUInt32(buffer, 0);
            readData += 4;

            CheckUnexpectedOutOfData(blockLength, readData);


            // Read the flags.
            byte flags = (byte)DataStream.ReadByte();
            readData += 1;

            CheckUnexpectedOutOfData(blockLength, readData);


            // If we haven't read the amount of data we were expected to read, there must
            // be a version mismatch. Consume the remaining data.
            if (readData < blockLength)
            {
                buffer = new byte[blockLength - readData];
                DataStream.Read(buffer, 0, buffer.Length);
            }

            // Create our header object.
            _header = new WappHeader()
            {
                FileId = fileIdentifier,
                Major = major,
                Minor = minor,
                BlockLength = blockLength,
                StringTableOffset = stringTableOffset,
                ChunkIndexOffset = chunkIndexOffset,
                PayloadOffset = payloadOffset,
                Flags = (HeaderFlags)flags,
            };
        }

        private void ReadStringTable()
        {
            byte numStrings = (byte)DataStream.ReadByte();
            for(int i = 0; i < numStrings; i++)
            {
                // Decode the length of the string.
                byte length = (byte)DataStream.ReadByte();

                // We multiply by two because the length will represent the string's length,
                // each character in the string takes two bytes, not one.
                byte[] buffer = new byte[length * 2];
                DataStream.Read(buffer, 0, length * 2);
                StringTable.Add(Encoding.Unicode.GetString(buffer));
            }
        }

        private void ReadChunkIndex()
        {
            // Read the number of chunks.
            byte[] buffer = new byte[2];
            DataStream.Read(buffer, 0, 2);
            _chunkIndex = new ChunkInfo[BitConverter.ToUInt16(buffer, 0)];

            for (int i = 0; i < _chunkIndex.Length; i++)
            {
                _chunkIndex[i] = ReadNextChunkInfo();
            }
        }

        private void ReadChunks(LayoutInfo targetLayout)
        {
            for (int i = 0; i < _chunkIndex.Length; i++)
            {
                var info = _chunkIndex[i];
                byte[] buffer = new byte[info.ChunkLength];
                DataStream.Read(buffer, 0, buffer.Length);

                uint crc = Force.Crc32.Crc32CAlgorithm.Compute(buffer);
                if(crc != info.ChunkHash)
                {
                    //TODO: Error
                    throw new InvalidDataException("Checksum mismatch.");
                }

                if(_header.PayloadCompressed)
                {
                    using(var inStream = new MemoryStream(buffer))
                    using (var gzip = new GZipStream(inStream, CompressionMode.Decompress))
                    using(var outStream = new MemoryStream())
                    {
                        gzip.CopyTo(outStream);
                        buffer = outStream.ToArray();
                    }
                }

                ChunkLoaders.ChunkLoading.Resolve(ref info, this, buffer, targetLayout);
            }
        }

        private ChunkInfo ReadNextChunkInfo()
        {
            // Read length.
            byte blockLength = (byte)DataStream.ReadByte();

            // Read the chunk's tag index
            byte stringIndex = (byte)DataStream.ReadByte();

            // Read the chunk's type ID.
            byte chunkType = (byte)DataStream.ReadByte();


            // Keep up with how many bytes we've consumed.
            byte readData = 3;


            // Read the offset from the payload start to the chunk's start.
            byte[] buffer = new byte[8];
            DataStream.Read(buffer, 0, 8);
            long chunkStart = BitConverter.ToInt64(buffer, 0);
            readData += 8;

            CheckUnexpectedOutOfData(blockLength, readData);

            // Read the chunk's length.
            DataStream.Read(buffer, 0, 8);
            long chunkLength = BitConverter.ToInt64(buffer, 0);
            readData += 8;

            CheckUnexpectedOutOfData(blockLength, readData);

            // Read the chunk's hash.
            DataStream.Read(buffer, 0, 4);
            uint hash = BitConverter.ToUInt32(buffer, 0);
            readData += 4;

            // If we haven't read the amount of data we were expected to read, there must
            // be a version mismatch. Consume the remaining data.
            if(readData < blockLength)
            {
                buffer = new byte[blockLength - readData];
                DataStream.Read(buffer, 0, buffer.Length);
            }

            // Create and return our object.
            return new ChunkInfo()
            {
                BlockLength = blockLength,
                TagIndex = stringIndex,
                ChunkType = chunkType,
                ChunkStartOffset = chunkStart,
                ChunkLength = chunkLength,
                ChunkHash = hash,
            };
        }

        private void CheckUnexpectedOutOfData(int expectedLength, int consumedData)
        {
            if(consumedData > expectedLength)
            {
                throw new InvalidDataException($"Expected {expectedLength} bytes, but consumed {consumedData}.");
            }
        }
    }
}
