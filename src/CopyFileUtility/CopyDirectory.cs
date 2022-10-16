using CopyFileUtility_Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public partial class CopyFileUtility
{
    public enum CopyStatus
    {
        Init,
        Running,
        Success,
        Fail,
    }
    public class CopyFileInfo
    {
        public CopyStatus CopyStatus { get; set; } = CopyStatus.Init;
        public string Src { get; set; } = string.Empty;
        public string Dst { get; set; } = string.Empty;
        public long FileSize = -1;
        public Exception? OccurredException { get; set; } = null;
    }

    public class CopyDirectoryProgress
    {
        public CopyFileInfo[] Files { get; set; } = Array.Empty<CopyFileInfo>();
        public CopyFileInfo? RunningFile { get; set; } = null;
        public long ReadedSize { get; set; } = -1;
        public long WritedSize { get; set; } = -1;
        public long FileSize => (RunningFile == null ? -1 : RunningFile.FileSize);

        internal void ResetRunning()
        {
            RunningFile = null;
            ReadedSize = -1;
            WritedSize = -1;
        }
        internal void SetRunningFile(CopyFileInfo info)
        {
            RunningFile = info;
            ReadedSize = 0;
            WritedSize = 0;
            info.CopyStatus = CopyStatus.Running;
        }
    }

    public static ValueTask<CopyFileInfo[]> CopyDirectoryAsync(
        string src,
        string dst,
        System.IO.SearchOption searchOption,
        CopyFileOptions fileOption,
        IProgress<CopyDirectoryProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return CopyDirectoryAsync(
            src,
            dst,
            (Regex?)null,
            (Regex?)null,
            null,
            searchOption,
            fileOption,
            progress,
            cancellationToken);
    }

    public static ValueTask<CopyFileInfo[]> CopyDirectoryAsync(
        string src,
        string dst,
        Func<string, string, string, string> changePathFunction,
        System.IO.SearchOption searchOption,
        CopyFileOptions fileOption,
        IProgress<CopyDirectoryProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return CopyDirectoryAsync(
            src,
            dst,
            (Regex?)null,
            (Regex?)null,
            changePathFunction,
            searchOption,
            fileOption,
            progress,
            cancellationToken);
    }

    public static ValueTask<CopyFileInfo[]> CopyDirectoryAsync(
        string src,
        string dst,
        string? includeSrcPathRegex,
        string? excludeSrcPathRegex,
        Func<string, string, string, string>? changePathFunction,
        System.IO.SearchOption searchOption,
        CopyFileOptions fileOption,
        IProgress<CopyDirectoryProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return CopyDirectoryAsync(
            src, 
            dst,
            string.IsNullOrEmpty(includeSrcPathRegex) ? null: new Regex(includeSrcPathRegex, RegexOptions.Compiled),
            string.IsNullOrEmpty(excludeSrcPathRegex) ? null : new Regex(excludeSrcPathRegex, RegexOptions.Compiled),
            changePathFunction,
            searchOption,
            fileOption,
            progress,
            cancellationToken);    
    }

    public static async ValueTask<CopyFileInfo[]> CopyDirectoryAsync(
        string src,
        string dst,
        Regex? includeSrcPathRegex,
        Regex? excludeSrcPathRegex,
        Func<string, string, string, string>? ChangePathFunction,
        System.IO.SearchOption searchOption,
        CopyFileOptions fileOption,
        IProgress<CopyDirectoryProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if(!System.IO.Directory.Exists(src))
        {
            throw new System.IO.DirectoryNotFoundException($"NotFound : {src}");
        }

        // SearchFiles
        var copyFiles = new List<CopyFileInfo>();
        foreach (var srcFile in System.IO.Directory.GetFiles(src, "*", searchOption))
        {
            var srcFileInfo = new System.IO.FileInfo(srcFile);
            var srcFullPath = srcFileInfo.FullName;
            if (includeSrcPathRegex != null && !includeSrcPathRegex.IsMatch(srcFullPath))
            {
                continue;
            }
            if (excludeSrcPathRegex != null && excludeSrcPathRegex.IsMatch(srcFullPath))
            {
                continue;
            }
            var relativePath = System.IO.Path.GetRelativePath(src, srcFullPath);
            var dstPath = System.IO.Path.Combine(dst, relativePath);
            var dstFullPath = System.IO.Path.GetFullPath(dstPath);
            // User Change FilePath
            if(ChangePathFunction != null)
            {
                dstFullPath = ChangePathFunction(srcFullPath,dstFullPath,relativePath);
                if(string.IsNullOrEmpty(dstFullPath))
                {
                    continue;
                }
            }

            // Add Copy
            copyFiles.Add(new CopyFileInfo()
            {
                Src = srcFullPath,
                Dst = dstFullPath,
                FileSize = srcFileInfo.Length,
            });
        }

        // MemoryPool
        var memoryPool = new ThreadMemoryPool(fileOption.BufferSize, fileOption.PoolSize);

        // Start Copy
        var report = new CopyDirectoryProgress()
        {
            Files = copyFiles.ToArray(),
        };
        progress?.Report(report);
        var copyFileProgress = new Progress<CopyFileProgress>(x =>
        {
            report.ReadedSize = x.ReadedSize;
            report.WritedSize = x.WritedSize;
            progress?.Report(report);
        });
        foreach (var fileInfo in copyFiles)
        {
            try
            {
                report.SetRunningFile(fileInfo);
                progress?.Report(report);
                await CopyFileAsync(memoryPool, fileInfo.Src, fileInfo.Dst, fileOption, copyFileProgress, cancellationToken);
                fileInfo.CopyStatus = CopyStatus.Success;
            }
            catch (Exception ex)
            {
                fileInfo.OccurredException = ex;
                fileInfo.CopyStatus = CopyStatus.Fail;
                progress?.Report(report);
            }
        }
        report.ResetRunning();
        progress?.Report(report);
        return copyFiles.ToArray();
    }
}
