using CP.Storage.Compressors.Parallelization;
using System;
using static CP.Storage.Constants.Sizes;

namespace CP.Storage.Compressors
{
    public static class CompressorFactory
    {
        public static ICompressor GetDeflateCompressor()
        {
            var deflateCompressor = new IonicDeflateCompressor();
            return deflateCompressor;
        }

        public static ICompressor GetParallelDeflateCompressor(IProgress<int> progress = null)
        {
            var deflateCompressor = new IonicDeflateCompressor();
            var parallelWrapperCompressor = new ParallelizationWrappingCompressor(deflateCompressor, progress, 1 * MB);
            return parallelWrapperCompressor;
        }
    }
}