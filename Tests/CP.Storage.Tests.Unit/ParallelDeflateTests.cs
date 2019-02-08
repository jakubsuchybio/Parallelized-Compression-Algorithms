using CP.Storage.Compressors;
using CP.Storage.Compressors.Parallelization;
using Moq;
using System;
using System.IO;
using Xunit;
using static CP.Storage.Constants.Sizes;

namespace CP.Storage.UnitTests
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
        public void CompressSmallerThanChunkSize()
        {
            // Assign
            int chunkSize = 1 * MB;
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
            int chunkSize = 1;
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

        [Fact]
        public void CompressLargerThanChunkSize()
        {
            // Assign
            int chunkSize = 2;
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
            int chunkSize = 1 * MB;
            var random = new Random();
            var buffer = new byte[chunkSize];
            random.NextBytes(buffer);

            var input = new MemoryStream(buffer);
            var compressed = new MemoryStream();
            var decompressed = new MemoryStream();

            var deflateCompressor = new IonicDeflateCompressor();
            var parallelCompressor = new ParallelizationWrappingCompressor(deflateCompressor, null, 1*KB);

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
