using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using System.IO;

namespace CP.Storage.Compressors
{
    public class LZ4Compressor : ICompressor
    {
        public string CompressedFileExtension => Constants.Extensions.LZ4;

        public void Compress(Stream source, Stream destination, int compressionLevel)
        {
            var settings = new LZ4EncoderSettings
            {
                CompressionLevel = (LZ4Level)compressionLevel
            };
            using (var compressionStream = LZ4Stream.Encode(destination, settings, leaveOpen: true))
            {
                source.CopyTo(compressionStream);
            }
        }

        public void Decompress(Stream source, Stream destination)
        {
            using(var compressionStream = LZ4Stream.Decode(destination, leaveOpen: true))
            {
                source.CopyTo(compressionStream);
            }
        }
    }
}