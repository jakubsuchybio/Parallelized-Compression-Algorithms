using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using ParallelCompression.Compressors;
using ParallelCompression.Parallelization;

namespace ParallelCompression.Benchmark
{
    [MemoryDiagnoser]
    public class ParallelVsSequentialCompressionOnly
    {
        public MemoryStream Input { get; set; }

        [Params(1 * Constants.Sizes.MB)] public int ChunkSize { get; set; }

        [Params(5, 9)] public int Compressionlevel { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            int chunkSize = 100 * Constants.Sizes.MB;
            var random = new Random();
            var buffer = new byte[chunkSize];
            random.NextBytes(buffer);

            Input = new MemoryStream(buffer);
        }

        [Benchmark]
        public void ParallelDeflate()
        {
            Input.Position = 0;
            var compressed = new MemoryStream();

            var deflateCompressor = new IonicDeflateCompressor();
            var parallelCompressor = new ParallelizationWrappingCompressor(deflateCompressor, null, ChunkSize);

            parallelCompressor.Compress(Input, compressed, Compressionlevel);
        }

        [Benchmark(Baseline = true)]
        public void Deflate()
        {
            Input.Position = 0;
            var compressed = new MemoryStream();

            var deflateCompressor = new IonicDeflateCompressor();

            deflateCompressor.Compress(Input, compressed, Compressionlevel);
        }
    }
}