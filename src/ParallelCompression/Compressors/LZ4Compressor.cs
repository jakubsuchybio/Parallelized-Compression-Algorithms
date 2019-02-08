using System.IO;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using ParallelCompression.Interfaces;

namespace ParallelCompression.Compressors
{
    public class LZ4Compressor : ICompressor
    {
        public string CompressedFileExtension => Constants.Extensions.LZ4;

        public void Compress(Stream source, Stream destination, int compressionLevel)
        {
            var settings = new LZ4EncoderSettings
            {
                CompressionLevel = (LZ4Level) compressionLevel
            };
            using (LZ4EncoderStream compressionStream = LZ4Stream.Encode(destination, settings, true))
            {
                source.CopyTo(compressionStream);
            }
        }

        public void Decompress(Stream source, Stream destination)
        {
            using (LZ4DecoderStream compressionStream = LZ4Stream.Decode(destination, leaveOpen: true))
            {
                source.CopyTo(compressionStream);
            }
        }
    }
}