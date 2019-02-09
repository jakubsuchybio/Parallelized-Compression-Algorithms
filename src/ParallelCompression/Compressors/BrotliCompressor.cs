using System.IO;
using System.IO.Compression;
using BrotliSharpLib;
using ParallelCompression.Interfaces;

namespace ParallelCompression.Compressors
{
    public class BrotliCompressor : ICompressor
    {
        public string CompressedFileExtension => Constants.Extensions.Brotli;

        public void Compress(Stream source, Stream destination, int compressionLevel)
        {
            using (var compressionStream = new BrotliStream(destination, CompressionMode.Compress, true))
            {
                compressionStream.SetQuality(compressionLevel);
                source.CopyTo(compressionStream);
            }
        }

        public void Decompress(Stream source, Stream destination)
        {
            using (var decompressionStream = new BrotliStream(source, CompressionMode.Decompress, true))
            {
                decompressionStream.CopyTo(destination);
            }
        }
    }
}