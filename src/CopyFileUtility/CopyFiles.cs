using CopyFileUtility_Internal;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;

public partial class CopyFileUtility
{
    public static ValueTask<CopyFileInfo[]> CopyFilesAsync(
        IEnumerable<string> srcFiles,
        IEnumerable<string> dstFiles,
        CopyFileOptions options,
        bool throwCopyException = false,
        IProgress<CopyFilesProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Check
        var srcFileArray = srcFiles.ToArray();
        var dstFileArray = dstFiles.ToArray();
        if (srcFileArray.Length != dstFileArray.Length)
        {
            throw new ArgumentException($"{nameof(srcFiles)}.Length != {nameof(dstFiles)}.Length");
        }

        // Create CopyInfo
        var copyFileInfos = new List<CopyFileInfo>(srcFileArray.Length);
        foreach (var index in Enumerable.Range(0, srcFileArray.Length))
        {
            copyFileInfos.Add(new CopyFileInfo()
            {
                Src = srcFileArray[index],
                Dst = dstFileArray[index],
                FileSize = new FileInfo(srcFileArray[index]).Length,
            });
        }

        // Copy
        return CopyFilesAsync(copyFileInfos.ToArray(), options, throwCopyException, progress, cancellationToken);
    }

    public static async ValueTask<CopyFileInfo[]> CopyFilesAsync(
        CopyFileInfo[] copyFiles,
        CopyFileOptions fileOption,
        bool throwCopyException = false,
        IProgress<CopyFilesProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // MemoryPool
        using var memoryPool = new MemoryPool(fileOption.BufferSize, fileOption.PoolSize);

        // Copy
        var report = new CopyFilesProgress()
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
        foreach (var (index, fileInfo) in copyFiles.Select((x, i) => (i, x)))
        {
            try
            {
                report.SetRunningFile(index);
                progress?.Report(report);
                await CopyFileAsync(memoryPool, fileInfo.Src, fileInfo.Dst, fileOption, copyFileProgress, cancellationToken).ConfigureAwait(false);
                fileInfo.CopyStatus = CopyStatus.Success;
            }
            catch (Exception ex)
            {
                fileInfo.OccurredException = ex;
                fileInfo.CopyStatus = CopyStatus.Fail;
                progress?.Report(report);

                // Exception
                if (throwCopyException)
                {
                    throw;
                }
            }
        }
        report.EndRunning();
        progress?.Report(report);
        return copyFiles.ToArray();
    }
}