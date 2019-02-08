using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using Xunit;

namespace CP.Storage.UnitTests
{
    public class ConcurrentDictionaryTests
    {
        [Fact]
        [Description(@"Given ConcurrentDictionary is empty, When I do TryRemove, Then it should return false and default MemoryStream value (null)")]
        public void TryRemove()
        {
            // Assign
            var concurrentDictionary = new ConcurrentDictionary<int, MemoryStream>();

            // Act
            bool result = concurrentDictionary.TryRemove(0, out var memoryStream);

            // Assert
            Assert.False(result);
            Assert.Equal(default(MemoryStream), memoryStream);
        }
    }
}