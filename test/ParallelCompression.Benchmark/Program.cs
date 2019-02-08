using System;

namespace ParallelCompression.Benchmark
{
    public static class Program
    {
        public static void Main()
        {
            //var summary = BenchmarkRunner.Run<ParallelVsSequentialCompressionOnly>();
            //var summary2 = BenchmarkRunner.Run<ParallelVsSequential>();
            //Console.WriteLine(summary);
            //Console.WriteLine(summary2);

            RealDataTest.Run();


            Console.ReadLine();
        }
    }
}