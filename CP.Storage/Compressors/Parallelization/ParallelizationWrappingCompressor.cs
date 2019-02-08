using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Buffers;

namespace CP.Storage.Compressors.Parallelization
{
    /// <summary>
    /// Wrapping compressor for parallelization of another compressor
    /// </summary>
    public class ParallelizationWrappingCompressor : ICompressor
    {
        private readonly ICompressor _wrappedCompressor;
        private readonly IProgress<int> _progress;
        private readonly int _chunkSize;

        private BlockingCollection<Chunk> _compressionChunks;
        private BlockingCollection<Chunk> _decompressionChunks;

        public string CompressedFileExtension =>
            _wrappedCompressor.CompressedFileExtension;

        public ParallelizationWrappingCompressor(ICompressor wrappedCompressor, IProgress<int> progress, int chunkSize = 1 * Constants.Sizes.MB)
        {
            _wrappedCompressor = wrappedCompressor;
            _progress = progress;
            _chunkSize = chunkSize;

            int degreeOfParalelization = Environment.ProcessorCount;

            _compressionChunks = new BlockingCollection<Chunk>(degreeOfParalelization);
            _decompressionChunks = new BlockingCollection<Chunk>(degreeOfParalelization);
        }

        public ParallelizationWrappingCompressor(ICompressor wrappedCompressor, IProgress<int> progress, int degreeOfParalelization, int chunkSize = 1 * Constants.Sizes.MB)
        {
            _wrappedCompressor = wrappedCompressor;
            _progress = progress;
            _chunkSize = chunkSize;

            _compressionChunks = new BlockingCollection<Chunk>(degreeOfParalelization);
            _decompressionChunks = new BlockingCollection<Chunk>(degreeOfParalelization);
        }

        /// <summary>
        /// Splits source stream into chunks, parallelizes computation of wrapped ICompressor on chunks and merges results to destination stream.
        /// It blocks until the source stream is fully compressed into destination stream
        /// </summary>
        /// <param name="source">Source stream that will be chunked and compressed in parallel</param>
        /// <param name="destination">Destination stream of merged parallel results</param>
        /// <param name="compressionLevel">Level of how much data are compressed</param>
        public void Compress(Stream source, Stream destination, int compressionLevel)
        {
            // Split source into chunks and compress
            var readingTask = Task.Run(() => ReadSourceIntoChunksAndCompress(source, compressionLevel));

            // Merge chunks
            var mergingTask = Task.Run(() => MergeCompressedChunks(destination));

            Task.WaitAll(readingTask, mergingTask);
        }

        public void Decompress(Stream source, Stream destination)
        {
            // Split source into chunks and decompress
            var readingTask = Task.Run(() => ReadSourceIntoChunksAndDecompress(source));

            // Merge chunks
            var mergingTask = Task.Run(() => MergeDecompressedChunks(destination));

            Task.WaitAll(readingTask, mergingTask);
        }

        private async Task ReadSourceIntoChunksAndCompress(Stream source, int compressionLevel)
        {
            int count = 1;
            int bytesRead;
            ArrayPool<byte> arrayPool = ArrayPool<byte>.Shared;
            while (source.Position < source.Length)
            {
                byte[] buffer = arrayPool.Rent(_chunkSize);
                bytesRead = await source.ReadAsync(buffer, 0, _chunkSize);
                var chunk = new Chunk
                {
                    Index = count++,
                    InputStream = new MemoryStream(buffer, 0, bytesRead)
                };
                _compressionChunks.Add(chunk);
                var _ = Task.Factory
                    .StartNew(() => CompressChunk(chunk, compressionLevel))
                    .ContinueWith(t => arrayPool.Return(buffer, true));


                _progress?.Report((int)(source.Position * 100 / source.Length));
            }
            _compressionChunks.CompleteAdding();
        }

        private void CompressChunk(Chunk chunk, int compressionLevel)
        {
            var output = new MemoryStream((int)chunk.InputStream.Length);
            _wrappedCompressor.Compress(chunk.InputStream, output, compressionLevel);
            chunk.OutputStream = output;
            chunk.Completed = true;
        }

        private async Task MergeCompressedChunks(Stream destination)
        {
            destination.WriteByte(ArchivationMethod.Parallel.ToByte());
            destination.WriteByte(ArchivationAlgorithm.Deflate.ToByte());

            while (_compressionChunks.Count != 0 || !_compressionChunks.IsAddingCompleted)
            {
                Chunk chunk;
                if (!_compressionChunks.TryTake(out chunk, -1))
                {
                    await Task.Delay(1);
                    continue;
                }

                while (!chunk.Completed)
                    await Task.Delay(1);

                int length = (int)chunk.OutputStream.Length;
                await destination.WriteAsync(BitConverter.GetBytes(length), 0, 4);
                await destination.WriteAsync(chunk.OutputStream.GetBuffer(), 0, length);
            }
        }

        private async Task ReadSourceIntoChunksAndDecompress(Stream source)
        {
            // Read header
            var parallelization = (ArchivationMethod)source.ReadByte();
            var method = (ArchivationAlgorithm)source.ReadByte();

            int count = 1;
            var lengthBuffer = new byte[4];

            ArrayPool<byte> arrayPool = ArrayPool<byte>.Shared;

            while (source.Position < source.Length)
            {
                // Read length of chunk
                await source.ReadAsync(lengthBuffer, 0, 4);
                int length = BitConverter.ToInt32(lengthBuffer, 0);

                // Read compressed chunk
                byte[] buffer = arrayPool.Rent(length);
                await source.ReadAsync(buffer, 0, length);
                var chunk = new Chunk
                {
                    Index = count++,
                    InputStream = new MemoryStream(buffer, 0, length)
                };
                _decompressionChunks.Add(chunk);

                var _ = Task.Factory
                    .StartNew(() => DecompressChunk(chunk))
                    .ContinueWith(t=>arrayPool.Return(buffer));
            }
            _decompressionChunks.CompleteAdding();
        }

        private void DecompressChunk(Chunk chunk)
        {
            var output = new MemoryStream((int)chunk.InputStream.Length);
            _wrappedCompressor.Decompress(chunk.InputStream, output);
            chunk.OutputStream = output;
            chunk.Completed = true;
        }

        private async Task MergeDecompressedChunks(Stream destination)
        {
            while (_decompressionChunks.Count != 0 || !_decompressionChunks.IsAddingCompleted)
            {
                Chunk chunk;
                if (!_decompressionChunks.TryTake(out chunk, -1))
                {
                    await Task.Delay(1);
                    continue;
                }

                while (!chunk.Completed)
                    await Task.Delay(1);

                await destination.WriteAsync(chunk.OutputStream.GetBuffer(), 0, (int)chunk.OutputStream.Length);
            }
        }
    }
}