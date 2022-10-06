using BenchmarkDotNet.Attributes;
using IOExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark
{
    [MemoryDiagnoser]
    //[ShortRunJob]
    public class Test
    {
        private static readonly long testFileSize = 3L * 1024L * 1024L * 1024L;
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
            srcFile = CreateFile(testFileSize);
            dstFile = System.IO.Path.GetTempFileName();
        }

        [Benchmark]
        public void DefaultCopy()
        {
            if (System.IO.File.Exists(dstFile))
            {
                System.IO.File.Delete(dstFile);
            }
            System.IO.File.Copy(srcFile, dstFile, true);
        }
        
        [Benchmark]
        public async Task BufferCopy()
        {
            if (System.IO.File.Exists(dstFile))
            {
                System.IO.File.Delete(dstFile);
            }
            var option = new CopyFileUtility.CopyFileOptions()
            {
                OverrideExistFile = true,
            };
            var progress = new Progress<CopyFileUtility.CopyFileProgress>(x =>
            {
                // Progress
                var readProgress = x.FileSize <= 0 ? 1.0 : (double)x.ReadedSize / (double)x.FileSize;
                var writeProgress = x.FileSize <= 0 ? 1.0 : (double)x.WritedSize / (double)x.FileSize;
            });
            await CopyFileUtility.CopyAsync(srcFile, dstFile, option, progress);
        }

        [Benchmark]
        public async Task FileTransferManagerCopy()
        {
            if (System.IO.File.Exists(dstFile))
            {
                System.IO.File.Delete(dstFile);
            }
            await FileTransferManager.CopyWithProgressAsync(srcFile, dstFile, (x) => {  }, true);
        }
    }
}
