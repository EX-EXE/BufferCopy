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

        private static string CreateFile(long fileSize)
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

        private static bool CompareFile(string pathA,string pathB)
        {
            if (!System.IO.File.Exists(pathA))
            {
                throw new System.IO.FileNotFoundException(pathA);
            }
            if (!System.IO.File.Exists(pathB))
            {
                throw new System.IO.FileNotFoundException(pathB);
            }

            using var streamA = new FileStream(pathA, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var streamB = new FileStream(pathB, FileMode.Open, FileAccess.Read, FileShare.Read);
            if(streamA.Length != streamB.Length)
            {
                return false;
            }

            var byteA = new Span<byte>(new byte[1024]);
            var byteB = new Span<byte>(new byte[1024]);
            while (streamA.Length != streamA.Position && streamB.Length != streamB.Position)
            {
                var readA = streamA.Read(byteA);
                var readB = streamB.Read(byteB);
                if(readA != readB || !byteA.SequenceEqual(byteB))
                {
                    return false;
                }
            }
            return streamA.Position == streamB.Position;
        }

        [Theory]
        [MemberData(nameof(FileSizeArray))]
        public async Task CopyFile(long fileSize)
        {
            var srcFile = CreateFile(fileSize);
            var dstFile = System.IO.Path.GetTempFileName();

            var option = new CopyFileOptions()
            {
                OverrideExistFile = true,
            };
            await CopyFileUtility.CopyFileAsync(srcFile, dstFile, option, null, default).ConfigureAwait(false);
            Assert.True(CompareFile(srcFile, dstFile));

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
                var srcFile = CreateFile(fileSize);
                await CopyFileUtility.CopyFileAsync(srcFile, dstFile, option, null, default).ConfigureAwait(false);
                Assert.True(CompareFile(srcFile, dstFile));
                System.IO.File.Delete(srcFile);
            }
            System.IO.File.Delete(dstFile);
        }
    }
}