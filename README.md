Normally compression algorithms are not parallel.<br>
In modern era of multi-core CPUs this is a problem.

Goal of this repository is to determine the best algorithm and parameters for given file type structure. e.g. Your application generates binary file in some format with some data. This library should help you find best algorithm with parameters best suited to your file structure type.

**This library contains 3 projects:**
- CP.Storage - This contains ICompressor interface, ParallelWrapper that wraps another compressor and parallelizes compression and decompression and few implementations of compression algorithms
- Tests/CP.Storage.Tests.Unit - Unit tests mainly for ParallelProcessor and some tests for combination of ParallelProcessor wrapping DeflateCompressor
- Tests/CP.Storage.Tests.Benchmark - This is a benchmark that tests speeds between non-parallel compressor and parallel compressor. On my CPU, there is mostly 2-5x speedup in compression and approx same speed with decompression

**Compression algorithms implemented as Compressors:**
- Deflate (System.IO.Compression from Microsoft)
- GZip (System.IO.Compression from Microsoft)
- Deflate (Ionic.Zlib nuget)
- GZip (Ionic.Zlib nuget)
- LZ4 (K4os.Compression.LZ4.Streams nuget)

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
1. Read input stream sequentially (first 4bytes is lenght of first chunk), create chunks and add them to the BlockingCollection
2. Start tasks for each chunk to decompress that chunk
3. Take chunks from BlockingCollection and store them into output stream

This way, we add a little overhead by saving multiple 4byte lengths of chunks, but we gain a lot speed by parallelizing workload of compression and we do not loose any decompression speed. We can lower the overhead of those lengths by increasing chunksize.
For the fastest speed and smallest overhead use chunksize:<br>
```(FileSize / CPUCount)```

**F.A.Q.:**
Q: Why using BlockingCollection?
A: By using BlockingCollection we throttle ussage of memory resources. It is because if we wouldn't throttle that, we could easily run out of memory when compressing some really large files (3GB+). We actualy do not throttle anything, because compressing and decompressing is much slower that file read/write, so the real bottleneck are tasks that are doing compression/decompression.