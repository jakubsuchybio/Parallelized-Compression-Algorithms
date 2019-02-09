using System.IO;
using System.IO.Compression;
using ParallelCompression.Interfaces;
using Zstandard.Net;

namespace ParallelCompression.Compressors
{
    public class ZStandardCompressor : ICompressor
    {
        public string CompressedFileExtension => Constants.Extensions.Brotli;

        public void Compress(Stream source, Stream destination, int compressionLevel)
        {
            using (var compressionStream = new ZstandardStream(destination, compressionLevel, true))
            {
                source.CopyTo(compressionStream);
            }
        }

        public void Decompress(Stream source, Stream destination)
        {
            using (var decompressionStream = new ZstandardStream(source, CompressionMode.Decompress, true))
            {
                decompressionStream.CopyTo(destination);
            }
        }
    }
}