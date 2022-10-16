using CopyFileUtility_Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public partial class CopyFileUtility
{
    public static ValueTask<CopyFileInfo[]> CopyDirectoryAsync(
        string src,
        string dst,
        System.IO.SearchOption searchOption,
        CopyFileOptions fileOption,
        bool throwCopyException = false,
        IProgress<CopyFilesProgress>? progress = null,
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
            throwCopyException,
            progress,
            cancellationToken);
    }

    public static ValueTask<CopyFileInfo[]> CopyDirectoryAsync(
        string src,
        string dst,
        Func<string, string, string, string> changePathFunction,
        System.IO.SearchOption searchOption,
        CopyFileOptions fileOption,
        bool throwCopyException = false,
        IProgress<CopyFilesProgress>? progress = null,
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
            throwCopyException,
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
        bool throwCopyException = false,
        IProgress<CopyFilesProgress>? progress = null,
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
            throwCopyException,
            progress,
            cancellationToken);    
    }

    public static ValueTask<CopyFileInfo[]> CopyDirectoryAsync(
        string src,
        string dst,
        Regex? includeSrcPathRegex,
        Regex? excludeSrcPathRegex,
        Func<string, string, string, string>? ChangePathFunction,
        System.IO.SearchOption searchOption,
        CopyFileOptions fileOption,
        bool throwCopyException = false,
        IProgress<CopyFilesProgress>? progress = null,
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
        return CopyFilesAsync(copyFiles.ToArray(), fileOption, throwCopyException, progress, cancellationToken);
    }
}
