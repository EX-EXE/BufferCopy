using BenchmarkDotNet.Running;

namespace Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<CopyFileBenchmark>();
            BenchmarkRunner.Run<CopyDirectoryBenchmark>();
        }
    }
}