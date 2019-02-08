using Ionic.Zlib;
using System.IO;

namespace CP.Storage.Compressors
{
    public class IonicDeflateCompressor : ICompressor
    {
        public string CompressedFileExtension => Constants.Extensions.Deflate;

        public void Compress(Stream source, Stream destination, int compressionLevel)
        {
            var zlipCompressionLevel = (CompressionLevel)compressionLevel;
            using (var compressionStream = new DeflateStream(source, CompressionMode.Compress, zlipCompressionLevel, true))
            {
                compressionStream.CopyTo(destination);
            }
        }

        public void Decompress(Stream source, Stream destination)
        {
            using (var compressionStream = new DeflateStream(source, CompressionMode.Decompress, true))
            {
                compressionStream.CopyTo(destination);
            }
        }
    }
}