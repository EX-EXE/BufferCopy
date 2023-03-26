using BenchmarkDotNet.Attributes;

namespace Benchmark
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class CopyFileBenchmark
    {
        public long TestFileSize { get; set; } = 1L * 1024L * 1024L * 1024L;

        private string srcFile = string.Empty;
        private string dstFile = string.Empty;

        public CopyFileBenchmark()
        {
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            srcFile = TestUtility.CreateFile(TestFileSize);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            dstFile = System.IO.Path.GetTempFileName();
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            if (System.IO.File.Exists(dstFile))
            {
                System.IO.File.Delete(dstFile);
            }
        }

        [Benchmark(Description = "System.IO.File.Copy")]
        public void DefaultCopy()
        {
            System.IO.File.Copy(srcFile, dstFile, true);
        }

        [Benchmark(Description = "CopyFileUtility.CopyFileAsync")]
        [Arguments(1024)]
        [Arguments(1024 * 128)]
        [Arguments(1024 * 256)]
        [Arguments(1024 * 512)]
        [Arguments(1024 * 1024)]
        public async Task CopyFileAsync(int buffer)
        {
            var option = new CopyFileUtility.CopyFileOptions()
            {
                OverrideExistFile = true,
                BufferSize = buffer,
            };
            var progress = new Progress<CopyFileUtility.CopyFileProgress>(x =>
            {
                // Progress
                var readProgress = x.FileSize <= 0 ? 1.0 : (double)x.ReadedSize / (double)x.FileSize;
                var writeProgress = x.FileSize <= 0 ? 1.0 : (double)x.WritedSize / (double)x.FileSize;
            });
            await CopyFileUtility.CopyFileAsync(srcFile, dstFile, option, progress);
        }
    }
}
