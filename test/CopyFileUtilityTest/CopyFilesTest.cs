using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace CopyFileUtilityTest
{
    public class CopyFilesTest
    {
        private readonly ITestOutputHelper output;
        public CopyFilesTest(ITestOutputHelper output)
        {
            this.output = output;
        }


        [Fact]
        public async Task CopyFiles()
        {
            // Create SrcFiles
            var (srcRootDir, srcFiles) = TestUtility.CreateFiles(128, 1, 1024, 1024 * 1024);
            var dstRootDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

            // Copy Files
            var dstList = new List<string>(srcFiles.Length);
            foreach(var srcFile in srcFiles)
            {
                var srcName = System.IO.Path.GetFileName(srcFile);
                dstList.Add(System.IO.Path.Combine(dstRootDir, srcName));
            }
            var dstFiles = dstList.ToArray();

            var option = new CopyFileUtility.CopyFileOptions()
            {
                OverrideExistFile = true,
            };
            await CopyFileUtility.CopyFilesAsync(srcFiles, dstFiles, option, false, null, default);

            // Check Files
            TestUtility.CompareFiles(srcFiles, dstList.ToArray());
            TestUtility.DeleteFiles(srcFiles, dstList);
        }

    }
}