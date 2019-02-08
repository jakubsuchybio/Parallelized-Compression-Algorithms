using System.IO;

namespace ParallelCompression.Interfaces
{
    public interface ICompressor
    {
        string CompressedFileExtension { get; }

        void Compress(Stream source, Stream destination, int compressionLevel);
        void Decompress(Stream source, Stream destination);
    }
}