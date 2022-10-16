using BenchmarkDotNet.Attributes;

namespace Benchmark
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class CopyDirectoryBenchmark
    {
        public int DirectoryDepth { get; set; } = 16;
        public int FileNum { get; set; } = 64;
        public int FileMinSize { get; set; } = 1024;
        public int FileMaxSize { get; set; } = 1024 * 1024 * 128;

        private string srcRoot = string.Empty;
        private string[] srcFiles = Array.Empty<string>();
        private string dstRoot = string.Empty;

        public CopyDirectoryBenchmark()
        {
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            (srcRoot, srcFiles) = TestUtility.CreateFiles(FileNum, DirectoryDepth, FileMinSize, FileMaxSize);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            dstRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            TestUtility.DeleteFiles(System.IO.Directory.GetFiles(dstRoot, "*", SearchOption.AllDirectories));
        }

        [Benchmark(Description = "Official CopyDirectory")]
        public void OfficialCopy()
        {
            // https://learn.microsoft.com/ja-jp/dotnet/standard/io/how-to-copy-directories
            void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
            {
                // Get information about the source directory
                var dir = new DirectoryInfo(sourceDir);

                // Check if the source directory exists
                if (!dir.Exists)
                    throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

                // Cache directories before we start copying
                DirectoryInfo[] dirs = dir.GetDirectories();

                // Create the destination directory
                Directory.CreateDirectory(destinationDir);

                // Get the files in the source directory and copy to the destination directory
                foreach (FileInfo file in dir.GetFiles())
                {
                    string targetFilePath = Path.Combine(destinationDir, file.Name);
                    file.CopyTo(targetFilePath);
                }

                // If recursive and copying subdirectories, recursively call this method
                if (recursive)
                {
                    foreach (DirectoryInfo subDir in dirs)
                    {
                        string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                        CopyDirectory(subDir.FullName, newDestinationDir, true);
                    }
                }
            }
            CopyDirectory(srcRoot, dstRoot, true);
        }

        [Benchmark(Description = "CopyFileUtility.CopyDirectoryAsync")]
        public async Task CopyDirectoryAsync()
        {
            var option = new CopyFileUtility.CopyFileOptions()
            {
                OverrideExistFile = true,
            };
            var progress = new Progress<CopyFileUtility.CopyFilesProgress>(x =>
            {
                // Progress
            });
            await CopyFileUtility.CopyDirectoryAsync(srcRoot, dstRoot, SearchOption.AllDirectories, option, false, progress);
        }
    }
}
