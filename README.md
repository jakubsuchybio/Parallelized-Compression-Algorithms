Normally compression algorithms are not parallel.<br>
In modern era of multi-core CPUs this is a problem.

Goal of this repository is to determine the best algorithm and parameters for given file type structure. e.g. Your application generates binary file in some format with some data. This library should help you find best algorithm with parameters best suited to your file structure type.

**[Here is a test results for 2GB binary file of ECG signal](TEST_RESULT_2GB_BINARY_FILE.md)**<br>
We are looking for the best CompressionRatio/Speed, which in this case is:

| Algorithm             | Chunk | Threads | CmprssLvl | CmprsRatio  | Speed [s] | MemoryMAX | MemoryAVG | 
|-----------------------|-------|---------|-----------|-------------|-----------|-----------|-----------|
| Parallel_IonicDeflate | 3MB   | 8       | 3         | 1.248183754 | 39.91     | 316.45 MB | 128.16 MB |
| Deflate               | 0     | 0       | Optimal   | 1.242586088 | 57.37     | 2.93 MB   | 2.87 MB   | 
| **Parallel_Deflate**      | **5MB**   | **8**       | **Optimal**   | **1.242418134** | **11.24**     | **507.45 MB** | **212.34 MB** |
      
Compared to the best Compression Ratio of 1.248183754 it is just slightly less.
Also compared to the non-parallel version of Deflate it is more than 5x faster on 8 threads. We use a lot of peak memory vs non-parallel version, but with today's PCs this is nothing.

**This library contains 3 projects:**
- src/ParallelizeCompression - This contains ICompressor interface, ParallelWrapper that wraps another compressor and parallelizes compression and decompression and few implementations of compression algorithms
- test/ParallelizeCompression.Tests.Unit - Unit tests mainly for ParallelProcessor and some tests for combination of ParallelProcessor wrapping DeflateCompressor
- test/ParallelizeCompression.Benchmark - This is a benchmark that tests speeds between non-parallel compressor and parallel compressor. On my CPU, there is mostly 2-5x speedup in compression and approx same speed with decompression

**Compression algorithms implemented as Compressors:**
- Deflate (System.IO.Compression from Microsoft)
- GZip (System.IO.Compression from Microsoft)
- Deflate (Ionic.Zlib nuget)
- GZip (Ionic.Zlib nuget)
- LZ4 (K4os.Compression.LZ4.Streams nuget)
- Brotli (BrotliSharpLib nuget)
- Zstandard (Zstandard.Net nuget)

**Parameters:**
- ChunkSize - Size of one chunk in bytes. (Optimal are one digits of MB (1MB,3MB,5MB))
- DegreeOfParallelization - How large is blocking collection and therefore how much tasks can run in at once. (Optimal is Environment.ProcessorCount)
- CompressionLevel - Dependant on algorithm used, but it says how much should given algorithm try to make file smaller for a cost of time.

**How parallelization of compression algorithms works:**<br>
Compression:
1. Read input stream sequentially, create chunks and add them to the BlockingCollection.
2. Start tasks for each chunk to compress that chunk
3. Take chunks from BlockingCollection and store them into output stream, BUT before writing chunk's output data, we first write it's length

Decompression:
1. Read input stream sequentially (first 4bytes is lenght of first chunk), create chunks and add them to the BlockingCollectio
2. Start tasks for each chunk to decompress that chunk
3. Take chunks from BlockingCollection and store them into output stream

This way, we add a little overhead by saving multiple 4byte lengths of chunks, but we gain a lot speed by parallelizing workload of compression and we do not loose any decompression speed. We can lower the overhead of those lengths by increasing chunksize.
For the fastest speed and smallest overhead use chunksize:<br>
```(FileSize / CPUCount)```

**F.A.Q.:**<br>
Q: Why using BlockingCollection?<br>
A: By using BlockingCollection we throttle ussage of memory resources. It is because if we wouldn't throttle that, we could easily run out of memory when compressing some really large files (3GB+). We actualy do not throttle anything, because compressing and decompressing is much slower that file read/write, so the real bottleneck are tasks that are doing compression/decompression.

Q: This is useless. Compressed files from this are not readable by any software! Why?<br>
A: Well, because we are bending the compression algorithms by compressing chunks of previous file. So this library is only usable for projects that do the compression and also the decompression themselves. You can't use this for compressing in your software and having someone else decompress it elsewere.
