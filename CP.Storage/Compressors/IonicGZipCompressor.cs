using Ionic.Zlib;
using System.IO;

namespace CP.Storage.Compressors
{
    public class IonicGZipCompressor : ICompressor
    {
        public string CompressedFileExtension => Constants.Extensions.GZip;

        public void Compress(Stream source, Stream destination, int compressionLevel)
        {
            var zlipCompressionLevel = (CompressionLevel)compressionLevel;
            using (var compressionStream = new GZipStream(destination, CompressionMode.Compress, zlipCompressionLevel, true))
            {
                source.CopyTo(compressionStream);
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