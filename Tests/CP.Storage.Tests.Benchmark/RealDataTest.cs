﻿using CP.Storage.BenchmarkTests.Utils;
using CP.Storage.Compressors;
using CP.Storage.Compressors.Parallelization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static CP.Storage.Constants.Sizes;

namespace CP.Storage.BenchmarkTests
{
    public class RealDataTest
    {
        public static void Run()
        {
            int[] chunkSizes = { 1 * MB, 2 * MB, 3 * MB, 4 * MB, 5 * MB };
            int[] degreesOfParalelization = { Environment.ProcessorCount, Environment.ProcessorCount * 2 };
            var compressorsAndCompressionLevels = new List<(ICompressor, int[])>
            {
                (new IonicGZipCompressor(), new [] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
                (new IonicDeflateCompressor(), new [] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
                (new GZipCompressor(), new [] { 2, 1, 0 }),
                (new DeflateCompressor(), new [] { 2, 1, 0 }),
                (new LZ4Compressor(), new [] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 })
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
                                Console.WriteLine($"Compressor: {compressor.GetType()} ChunkSize: {chunkSize} Parallelization: {degreeOfParalelization} CompressLevel: {compressionlevel} CompressRatio: {(double)input.Length / compressed.Length} Speed: {sw.Elapsed.TotalSeconds} MemMAX: {mw.MaximumMemoryAllocation} MemAVG: {mw.AverageMemoryAllocation}");
                            }
                        }
        }
    }
}