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
    [ShortRunJob]
    public class Test
    {
        private readonly static string SrcFile = @"C:\Users\EXE\Downloads\ubuntu-22.04.1-desktop-amd64.iso";
        private readonly static string DstFile = @"D:\Temp\TestData.bin";

        public Test()
        {
        }

        [Benchmark]
        public void DefaultCopy()
        {
            if (System.IO.File.Exists(DstFile))
            {
                System.IO.File.Delete(DstFile);
            }
            System.IO.File.Copy(SrcFile, DstFile, true);
        }

        [Benchmark]
        public async Task BufferCopy()
        {
            if (System.IO.File.Exists(DstFile))
            {
                System.IO.File.Delete(DstFile);
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
            await CopyFileUtility.CopyAsync(SrcFile, DstFile, option, progress);
        }

        [Benchmark]
        public async Task FileTransferManagerCopy()
        {
            if (System.IO.File.Exists(DstFile))
            {
                System.IO.File.Delete(DstFile);
            }
            await FileTransferManager.CopyWithProgressAsync(SrcFile, DstFile, (x) => {  }, true);
        }
    }
}
