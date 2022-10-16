using BenchmarkDotNet.Attributes;

namespace Benchmark
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class Test
    {

        public long TestFileSize { get; set; } = 1L * 1024L * 1024L * 1024L;

        private string srcFile = string.Empty;
        private string dstFile = string.Empty;

        public Test()
        {
            string CreateFile(long fileSize)
            {
                var path = System.IO.Path.GetTempFileName();
                using var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                stream.SetLength(fileSize);

                var maxWriteSize = int.MaxValue / 2;
                while (0 < fileSize)
                {
                    var createSize = maxWriteSize < fileSize ? maxWriteSize : fileSize;
                    fileSize -= createSize;
                    var data = new byte[createSize];
                    var rnd = new Random((int)DateTime.Now.Ticks);
                    rnd.NextBytes(data);
                    stream.Write(data);
                }
                return path;
            }
            srcFile = CreateFile(TestFileSize);
            dstFile = System.IO.Path.GetTempFileName();
        }

        [Benchmark(Description = "System.IO.File.Copy")]
        public void DefaultCopy()
        {
            if (System.IO.File.Exists(dstFile))
            {
                System.IO.File.Delete(dstFile);
            }
            System.IO.File.Copy(srcFile, dstFile, true);
        }
        
        [Benchmark]
        [Arguments(16, 8)]
        [Arguments(256, 8)]
        [Arguments(1024, 8)]
        [Arguments(1024 * 1024, 8)]
        [Arguments(16, 16)]
        [Arguments(256, 16)]
        [Arguments(1024, 16)]
        [Arguments(1024 * 1024, 16)]
        [Arguments(16, 30)]
        [Arguments(256, 30)]
        [Arguments(1024, 30)]
        [Arguments(1024 * 1024, 30)]
        public async Task BufferCopy(int buffer,int pool)
        {
            if (System.IO.File.Exists(dstFile))
            {
                System.IO.File.Delete(dstFile);
            }
            var option = new CopyFileUtility.CopyFileOptions()
            {
                OverrideExistFile = true,
                BufferSize = buffer,
                PoolSize = pool,
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
