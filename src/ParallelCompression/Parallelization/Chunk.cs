using System.IO;

namespace ParallelCompression.Parallelization
{
    public class Chunk
    {
        public int Index { get; set; }
        public MemoryStream InputStream { get; set; }
        public MemoryStream OutputStream { get; set; }
        public bool Completed { get; set; }
    }
}