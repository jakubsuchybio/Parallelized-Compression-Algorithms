using System;
using System.IO;
using ParallelCompression.Compressors;
using ParallelCompression.Parallelization;
using Xunit;

namespace ParallelCompression.Tests.Unit
{
    public class ParallelDeflateTests
    {
        [Fact]
        public void CompressEmpty()
        {
            // Assign
            var input = new MemoryStream();
            var compressed = new MemoryStream();
            var decompressed = new MemoryStream();

            var deflateCompressor = new IonicDeflateCompressor();
            var parallelCompressor = new ParallelizationWrappingCompressor(deflateCompressor, null);

            // Act
            parallelCompressor.Compress(input, compressed, 0);
            parallelCompressor.Decompress(compressed, decompressed);

            // Assert
            Assert.True(input.Length == 0);
            Assert.True(decompressed.Length == 0);
        }

        [Fact]
        public void CompressLargerThanChunkSize()
        {
            // Assign
            var chunkSize = 2;
            var random = new Random();
            var buffer = new byte[chunkSize + 1];
            random.NextBytes(buffer);

            var input = new MemoryStream(buffer);
            var compressed = new MemoryStream();
            var decompressed = new MemoryStream();

            var deflateCompressor = new IonicDeflateCompressor();
            var parallelCompressor = new ParallelizationWrappingCompressor(deflateCompressor, null, chunkSize);

            // Act
            parallelCompressor.Compress(input, compressed, 0);
            compressed.Position = 0;
            parallelCompressor.Decompress(compressed, decompressed);

            // Assert
            Assert.True(input.Length == chunkSize + 1);
            Assert.True(decompressed.Length == chunkSize + 1);
            Assert.Equal(input.ToArray(), decompressed.ToArray());
        }

        [Fact]
        public void CompressMultipleTimesLargerThanChunkSize()
        {
            // Assign
            int chunkSize = 1 * Constants.Sizes.MB;
            var random = new Random();
            var buffer = new byte[chunkSize];
            random.NextBytes(buffer);

            var input = new MemoryStream(buffer);
            var compressed = new MemoryStream();
            var decompressed = new MemoryStream();

            var deflateCompressor = new IonicDeflateCompressor();
            var parallelCompressor = new ParallelizationWrappingCompressor(deflateCompressor, null, 1 * Constants.Sizes.KB);

            // Act
            parallelCompressor.Compress(input, compressed, 0);
            compressed.Position = 0;
            parallelCompressor.Decompress(compressed, decompressed);

            // Assert
            Assert.True(input.Length == chunkSize);
            Assert.True(decompressed.Length == chunkSize);
            Assert.Equal(input.ToArray(), decompressed.ToArray());
        }

        [Fact]
        public void CompressSmallerThanChunkSize()
        {
            // Assign
            int chunkSize = 1 * Constants.Sizes.MB;
            var random = new Random();
            var buffer = new byte[chunkSize - 1];
            random.NextBytes(buffer);

            var input = new MemoryStream(buffer);
            var compressed = new MemoryStream();
            var decompressed = new MemoryStream();

            var deflateCompressor = new IonicDeflateCompressor();
            var parallelCompressor = new ParallelizationWrappingCompressor(deflateCompressor, null, chunkSize);

            // Act
            parallelCompressor.Compress(input, compressed, 0);
            compressed.Position = 0;
            parallelCompressor.Decompress(compressed, decompressed);

            // Assert
            Assert.True(input.Length == chunkSize - 1);
            Assert.True(decompressed.Length == chunkSize - 1);
            Assert.Equal(input.ToArray(), decompressed.ToArray());
        }

        [Fact]
        public void CompressSmallestChunkSize()
        {
            // Assign
            var chunkSize = 1;
            var random = new Random();
            var buffer = new byte[1];
            random.NextBytes(buffer);

            var input = new MemoryStream(buffer);
            var compressed = new MemoryStream();
            var decompressed = new MemoryStream();

            var deflateCompressor = new IonicDeflateCompressor();
            var parallelCompressor = new ParallelizationWrappingCompressor(deflateCompressor, null);

            // Act
            parallelCompressor.Compress(input, compressed, 0);
            compressed.Position = 0;
            parallelCompressor.Decompress(compressed, decompressed);

            // Assert
            Assert.True(input.Length == chunkSize);
            Assert.True(decompressed.Length == chunkSize);
            Assert.Equal(input.ToArray(), decompressed.ToArray());
        }
    }
}