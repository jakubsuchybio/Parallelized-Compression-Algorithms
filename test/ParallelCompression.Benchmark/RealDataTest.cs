using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ParallelCompression.Benchmark.Utils;
using ParallelCompression.Compressors;
using ParallelCompression.Interfaces;
using ParallelCompression.Parallelization;

namespace ParallelCompression.Benchmark
{
    public class RealDataTest
    {
        public static void Run()
        {
            int[] chunkSizes = {1 * Constants.Sizes.MB, 2 * Constants.Sizes.MB, 3 * Constants.Sizes.MB, 4 * Constants.Sizes.MB, 5 * Constants.Sizes.MB};
            int[] degreesOfParalelization = {Environment.ProcessorCount};
            var compressorsAndCompressionLevels = new List<(ICompressor, int[])>
            {
                (new IonicGZipCompressor(), new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}),
                (new IonicDeflateCompressor(), new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}),
                (new GZipCompressor(), new[] {2, 1, 0}),
                (new DeflateCompressor(), new[] {2, 1, 0}),
                (new LZ4Compressor(), new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12}),
                (new BrotliCompressor(), new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11}),
                (new ZStandardCompressor(), new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22})
            };

            foreach (var (compressor, compressionLevels) in compressorsAndCompressionLevels)
            foreach (int degreeOfParalelization in degreesOfParalelization)
            foreach (int chunkSize in chunkSizes)
            foreach (int compressionlevel in compressionLevels)
            {
                var sw = new Stopwatch();
                var mw = new MemoryWatch();

                File.Delete(@"c:\Users\info\Downloads\zrychleni\compressed.ERD");

                using (var input = new FileStream(@"c:\Users\info\Downloads\zrychleni\H_b3ef3991-90df-41a9-bf8e-a44f9066f6e9.ERD", FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var compressed = new FileStream(@"c:\Users\info\Downloads\zrychleni\compressed.ERD", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                {
                    var parallelCompressor = new ParallelizationWrappingCompressor(compressor, null, degreeOfParalelization, chunkSize);

                    sw.Start();
                    mw.Start();
                    parallelCompressor.Compress(input, compressed, compressionlevel);
                    mw.Stop();
                    sw.Stop();
                    Console.WriteLine($"ParallelCompressor: {compressor.GetType()} ChunkSize: {chunkSize} Parallelization: {degreeOfParalelization} CompressLevel: {compressionlevel} CompressRatio: {(double) input.Length / compressed.Length} Speed: {sw.Elapsed.TotalSeconds} MemMAX: {mw.MaximumMemoryAllocation} MemAVG: {mw.AverageMemoryAllocation}");
                }
            }

            foreach (var (compressor, compressionLevels) in compressorsAndCompressionLevels)
            foreach (int compressionlevel in compressionLevels)
            {
                var sw = new Stopwatch();
                var mw = new MemoryWatch();

                File.Delete(@"c:\Users\info\Downloads\zrychleni\compressed.ERD");

                using (var input = new FileStream(@"c:\Users\info\Downloads\zrychleni\H_b3ef3991-90df-41a9-bf8e-a44f9066f6e9.ERD", FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var compressed = new FileStream(@"c:\Users\info\Downloads\zrychleni\compressed.ERD", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                {
                    sw.Start();
                    mw.Start();
                    compressor.Compress(input, compressed, compressionlevel);
                    mw.Stop();
                    sw.Stop();
                    Console.WriteLine($"Compressor: {compressor.GetType()} ChunkSize: {0} Parallelization: {0} CompressLevel: {compressionlevel} CompressRatio: {(double) input.Length / compressed.Length} Speed: {sw.Elapsed.TotalSeconds} MemMAX: {mw.MaximumMemoryAllocation} MemAVG: {mw.AverageMemoryAllocation}");
                }
            }
        }
    }
}