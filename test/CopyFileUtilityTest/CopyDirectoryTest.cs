using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace CopyFileUtilityTest
{
    public class CopyDirectoryTest
    {
        private readonly ITestOutputHelper output;
        public CopyDirectoryTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        private void CheckSrcFiles(string[] srcFiles, CopyFileUtility.CopyFileInfo[] copyFileInfos)
        {
            // Check SrcFiles
            var replaceSrcFiles = srcFiles.Select(x => x.Replace('\\', '/')).ToArray();
            var dstSrcFiles = copyFileInfos.Select(x => x.Src.Replace('\\', '/')).ToArray();
            if (replaceSrcFiles.Length != dstSrcFiles.Length ||
                !replaceSrcFiles.Where(srcFile => dstSrcFiles.Where(dstFile => dstFile.Equals(srcFile, StringComparison.OrdinalIgnoreCase)).Any()).Any())
            {
                throw new Exception("Check SrcFiles");
            }
        }

        [Fact]
        public async Task CopyDirectory()
        {
            // Create SrcFiles
            var (srcRootDir, srcFiles) = TestUtility.CreateFiles(128, 16, 1024, 1024 * 1024, output);
            var dstRootDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

            // Copy Files
            var option = new CopyFileUtility.CopyFileOptions()
            {
                OverrideExistFile = true,
            };
            var dstFileInfos = await CopyFileUtility.CopyDirectoryAsync(srcRootDir, dstRootDir, SearchOption.AllDirectories, option, false, null, default);

            // Check Files
            CheckSrcFiles(srcFiles, dstFileInfos);
            TestUtility.CompareFiles(dstFileInfos.Select(x => x.Src).ToArray(), dstFileInfos.Select(x => x.Dst).ToArray());
        }


        [Fact]
        public async Task CopyDirectoryChangeFilePath()
        {
            // Create SrcFiles
            var (srcRootDir, srcFiles) = TestUtility.CreateFiles(128, 16, 1024, 1024 * 1024, output);
            var dstRootDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

            // Copy Files
            var option = new CopyFileUtility.CopyFileOptions()
            {
                OverrideExistFile = true,
            };
            var index = 0;
            var dstFileInfos = await CopyFileUtility.CopyDirectoryAsync(srcRootDir, dstRootDir,
                (string src, string dst, string _) =>
                {
                    return System.IO.Path.Combine(dstRootDir, (index++).ToString().PadLeft(3, '0') + ".dat");
                }, SearchOption.AllDirectories, option, false, null, default);

            // Check Files
            CheckSrcFiles(srcFiles, dstFileInfos);
            TestUtility.CompareFiles(dstFileInfos.Select(x => x.Src).ToArray(), dstFileInfos.Select(x => x.Dst).ToArray());
            TestUtility.DeleteFiles(srcFiles, dstFileInfos.Select(x => x.Dst));
        }

        [Fact]
        public async Task CopyDirectoryIncludeRegex()
        {
            // Create SrcFiles
            var (srcRootDir, srcFiles) = TestUtility.CreateFiles(128, 16, 1024, 1024 * 1024, output);
            var dstRootDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

            var singleFiles = System.IO.Path.GetFileName(srcFiles[0]);

            // Copy Files
            var option = new CopyFileUtility.CopyFileOptions()
            {
                OverrideExistFile = true,
            };
            var dstFileInfos = await CopyFileUtility.CopyDirectoryAsync(srcRootDir, dstRootDir, Regex.Escape(singleFiles), null, null, SearchOption.AllDirectories, option, false, null, default);

            // Check Files
            var srcFilterFiles = System.IO.Directory.GetFiles(srcRootDir, singleFiles, SearchOption.AllDirectories);
            CheckSrcFiles(srcFilterFiles, dstFileInfos);
            TestUtility.CompareFiles(dstFileInfos.Select(x => x.Src).ToArray(), dstFileInfos.Select(x => x.Dst).ToArray());
            TestUtility.DeleteFiles(srcFiles, dstFileInfos.Select(x => x.Dst));
        }

        [Fact]
        public async Task CopyDirectoryExcludeRegex()
        {
            // Create SrcFiles
            var (srcRootDir, srcFiles) = TestUtility.CreateFiles(128, 16, 1024, 1024 * 1024, output);
            var dstRootDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

            var singleFileName = System.IO.Path.GetFileName(srcFiles[0]);

            // Copy Files
            var option = new CopyFileUtility.CopyFileOptions()
            {
                OverrideExistFile = true,
            };
            var dstFileInfos = await CopyFileUtility.CopyDirectoryAsync(srcRootDir, dstRootDir, null, Regex.Escape(singleFileName), null, SearchOption.AllDirectories, option, false, null, default);

            // Check Files
            var srcFilterFiles = System.IO.Directory.GetFiles(srcRootDir, "*", SearchOption.AllDirectories)
                .Where(x => !System.IO.Path.GetFileName(x).Equals(singleFileName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            CheckSrcFiles(srcFilterFiles, dstFileInfos);
            TestUtility.CompareFiles(dstFileInfos.Select(x => x.Src).ToArray(), dstFileInfos.Select(x => x.Dst).ToArray());
            TestUtility.DeleteFiles(srcFiles, dstFileInfos.Select(x => x.Dst));
        }
    }
}