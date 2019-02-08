using BenchmarkDotNet.Attributes;
using CP.Storage.Compressors;
using CP.Storage.Compressors.Parallelization;
using System;
using System.IO;
using static CP.Storage.Constants.Sizes;

namespace CP.Storage.BenchmarkTests
{
    [MemoryDiagnoser]
    public class ParallelVsSequentialCompressionOnly
    {
        public MemoryStream Input { get; set; }

        [Params(1 * MB)]
        public int ChunkSize { get; set; }

        [Params(5, 9)]
        public int Compressionlevel { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            int chunkSize = 100 * MB;
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
