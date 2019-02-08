using System.IO;
using System.IO.Compression;

namespace CP.Storage.Compressors
{
    public class GZipCompressor : ICompressor
    {
        public string CompressedFileExtension => Constants.Extensions.GZip;

        public void Compress(Stream source, Stream destination, int compressionLevel)
        {
            var compressionLevelInner = (CompressionLevel)compressionLevel;
            using (var compressionStream = new GZipStream(destination, compressionLevelInner, true))
            {
                source.CopyTo(compressionStream);
            }
        }

        public void Decompress(Stream source, Stream destination)
        {
            using (var decompressionStream = new GZipStream(destination, CompressionMode.Decompress, true))
            {
                source.CopyTo(decompressionStream);
            }
        }
    }
}