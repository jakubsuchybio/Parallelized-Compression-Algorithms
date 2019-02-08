using System.IO;

namespace CP.Storage.Compressors
{
    public interface ICompressor
    {
        string CompressedFileExtension { get; }

        void Compress(Stream source, Stream destination, int compressionLevel);
        void Decompress(Stream source, Stream destination);
    }
}