using System.IO;
using System.IO.Compression;

namespace CP.Storage.Compressors
{
    public class DeflateCompressor : ICompressor
    {
        public string CompressedFileExtension => Constants.Extensions.Deflate;

        public void Compress(Stream source, Stream destination, int compressionLevel)
        {
            var compressionLevelInner = (CompressionLevel)compressionLevel;
            using (var compressionStream = new DeflateStream(destination, compressionLevelInner, true))
            {
                source.CopyTo(compressionStream);
            }
        }

        public void Decompress(Stream source, Stream destination)
        {
            using (var decompressionStream = new DeflateStream(destination, CompressionMode.Decompress, true))
            {
                source.CopyTo(decompressionStream);
            }
        }
    }
}