using System;
using System.Buffers;
using System.IO;
using static CopyFileUtility;

namespace CopyFileUtilityTest
{
    public class CopyFileTest
    {
        private static readonly long[] Nums = { 0, 1, 2, 8, 16, 32, 64, 128, 256, 512, 1024, 5, 10, 100, 500, 1000, 2000, 5000 };
        private static readonly long[] Units = { 1, 1024, 1024 * 1024 };

        private static List<object[]> FileSizeArray()
        {
            var ret = new List<object[]>();
            foreach (var unit in Units)
            {
                foreach (var num in Nums)
                {
                    ret.Add(new object[] { num * unit });
                }
            }
            return ret;
        }


        [Theory]
        [MemberData(nameof(FileSizeArray))]
        public async Task CopyFile(long fileSize)
        {
            var srcFile = TestUtility.CreateFile(fileSize);
            var dstFile = System.IO.Path.GetTempFileName();

            var option = new CopyFileOptions()
            {
                OverrideExistFile = true,
            };
            await CopyFileUtility.CopyFileAsync(srcFile, dstFile, option, null, default).ConfigureAwait(false);
            Assert.True(TestUtility.CompareFile(srcFile, dstFile));

            System.IO.File.Delete(dstFile);
            System.IO.File.Delete(srcFile);
        }

        [Fact]
        public async Task CopyOverride()
        {
            var dstFile = System.IO.Path.GetTempFileName();
            var option = new CopyFileOptions()
            {
                OverrideExistFile = true,
            };
            foreach (var fileSize in new[] { 1024, 1024 * 1024, 1024 * 2 })
            {
                var srcFile = TestUtility.CreateFile(fileSize);
                await CopyFileUtility.CopyFileAsync(srcFile, dstFile, option, null, default).ConfigureAwait(false);
                Assert.True(TestUtility.CompareFile(srcFile, dstFile));
                System.IO.File.Delete(srcFile);
            }
            System.IO.File.Delete(dstFile);
        }
    }
}