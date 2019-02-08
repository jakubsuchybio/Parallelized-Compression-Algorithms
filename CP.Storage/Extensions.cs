using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CP.Storage
{
    public static class Extensions
    {
        public static int ToIntCompressionLevel(this ArchivationSpeed archivationSpeed)
        {
            switch (archivationSpeed)
            {
                case ArchivationSpeed.Fastest:
                    return 0;
                case ArchivationSpeed.Average:
                    return 5;
                case ArchivationSpeed.Best:
                    return 9;
                default:
                    throw new ArgumentOutOfRangeException(nameof(archivationSpeed), archivationSpeed, null);
            }
        }

        public static byte ToByte(this ArchivationMethod method) =>
            (byte) method;
        public static byte ToByte(this ArchivationAlgorithm algorithm) =>
            (byte) algorithm;
    }
}
