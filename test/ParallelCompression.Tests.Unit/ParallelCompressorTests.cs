using System;
using System.IO;
using Moq;
using ParallelCompression.Interfaces;
using ParallelCompression.Parallelization;
using Xunit;

namespace ParallelCompression.Tests.Unit
{
    public class ParallelCompressorTests
    {
        [Fact]
        public void CompressEmpty()
        {
            // Assign
            var input = new MemoryStream();
            var output = new MemoryStream();
            var wrappedCompressor = new Mock<ICompressor>();
            wrappedCompressor
                .Setup(x => x.Compress(It.IsAny<MemoryStream>(), It.IsAny<MemoryStream>(), It.IsAny<int>()))
                .Callback((Stream source, Stream destination, int compressionLevel) => source.CopyTo(destination));
            var parallelCompressor = new ParallelizationWrappingCompressor(wrappedCompressor.Object, null);

            // Act
            parallelCompressor.Compress(input, output, 0);

            // Assert
            Assert.True(input.Length == 0);
            Assert.True(output.Length == 0);
        }

        [Fact]
        public void CompressLargerThanChunkSize()
        {
            // Assign
            int chunkSize = 1 * Constants.Sizes.MB;
            var random = new Random();
            var buffer = new byte[chunkSize + 1];
            random.NextBytes(buffer);
            var input = new MemoryStream(buffer);
            var output = new MemoryStream();
            var wrappedCompressor = new Mock<ICompressor>();
            wrappedCompressor
                .Setup(x => x.Compress(It.IsAny<MemoryStream>(), It.IsAny<MemoryStream>(), It.IsAny<int>()))
                .Callback((Stream source, Stream destination, int compressionLevel) => source.CopyTo(destination));
            var parallelCompressor = new ParallelizationWrappingCompressor(wrappedCompressor.Object, null, chunkSize);

            // Act
            parallelCompressor.Compress(input, output, 0);

            // Assert
            Assert.True(input.Length == chunkSize + 1);
            Assert.True(output.Length == chunkSize + 1);
        }

        [Fact]
        public void CompressMultipleTimesLargerThanChunkSize()
        {
            // Assign
            int chunkSize = 10 * Constants.Sizes.MB;
            var random = new Random();
            var buffer = new byte[chunkSize];
            random.NextBytes(buffer);
            var input = new MemoryStream(buffer);
            var output = new MemoryStream();
            var wrappedCompressor = new Mock<ICompressor>();
            wrappedCompressor
                .Setup(x => x.Compress(It.IsAny<MemoryStream>(), It.IsAny<MemoryStream>(), It.IsAny<int>()))
                .Callback((Stream source, Stream destination, int compressionLevel) => source.CopyTo(destination));
            var parallelCompressor = new ParallelizationWrappingCompressor(wrappedCompressor.Object, null, 1 * Constants.Sizes.MB);

            // Act
            parallelCompressor.Compress(input, output, 0);

            // Assert
            Assert.True(input.Length == chunkSize);
            Assert.True(output.Length == chunkSize);
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
            var output = new MemoryStream();
            var wrappedCompressor = new Mock<ICompressor>();
            wrappedCompressor
                .Setup(x => x.Compress(It.IsAny<MemoryStream>(), It.IsAny<MemoryStream>(), It.IsAny<int>()))
                .Callback((Stream source, Stream destination, int compressionLevel) => source.CopyTo(destination));
            var parallelCompressor = new ParallelizationWrappingCompressor(wrappedCompressor.Object, null, chunkSize);

            // Act
            parallelCompressor.Compress(input, output, 0);

            // Assert
            Assert.True(input.Length == chunkSize - 1);
            Assert.True(output.Length == chunkSize - 1);
        }
    }
}