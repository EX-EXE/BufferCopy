using CopyFileUtility_Internal;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

public partial class CopyFileUtility
{
    public static async ValueTask CopyFileAsync(
        string src,
        string dst,
        CopyFileOptions option,
        IProgress<CopyFileProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Memory
        var srcFileSize = new System.IO.FileInfo(src.ToString()).Length;
        var bufferSize = srcFileSize < option.BufferSize ? srcFileSize : option.BufferSize;
        using var memoryPool = new MemoryPool((int)bufferSize);
        // Copy
        await CopyFileAsync(memoryPool, src, dst, option, progress, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask CopyFileAsync(
        MemoryPool memoryPool,
         string src,
         string dst,
         CopyFileOptions option,
         IProgress<CopyFileProgress>? progress = null,
         CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Check
        if (!System.IO.File.Exists(src))
        {
            throw new System.IO.FileNotFoundException($"NotFound SrcFile : {src}");
        }
        if (System.IO.File.Exists(dst))
        {
            if (!option.OverrideExistFile)
            {
                throw new InvalidOperationException($"Exist DstFile : {dst}");
            }
            System.IO.File.SetAttributes(dst, FileAttributes.Normal);
            System.IO.File.Delete(dst);
        }
        // Token
        using var linkedCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var linkedCancelToken = linkedCancelTokenSource.Token;

        // Report
        var reportInfo = new CopyFileProgress()
        {
            StartDate = DateTime.Now,
            FileSize = new System.IO.FileInfo(src.ToString()).Length
        };
        var progressTask = Task.Run(async () =>
        {
            if (progress == null)
            {
                return;
            }
            while (true)
            {
                progress.Report(reportInfo);
                if (linkedCancelToken.IsCancellationRequested)
                {
                    return;
                }
                try
                {
                    await Task.Delay(option.ReportInterval, linkedCancelToken).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    // None
                }
            }
        }, linkedCancelToken);

        // MemoryPool
        memoryPool.Reset(true);

        // Channel
        var channelOption = new UnboundedChannelOptions()
        {
            AllowSynchronousContinuations = false,
            SingleReader = true,
            SingleWriter = true,
        };
        var writeChannel = Channel.CreateUnbounded<(MemoryEx<byte> memory,int readSize)>(channelOption);

        // WriteTask
        var writeTask = Task.Run(async () =>
        {
            try
            {
                // CreateDirectory
                var dstParentDir = System.IO.Path.GetDirectoryName(dst);
                if (!string.IsNullOrEmpty(dstParentDir) && !System.IO.Directory.Exists(dstParentDir))
                {
                    System.IO.Directory.CreateDirectory(dstParentDir);
                }

                // WriteFile
                using var writeStream = new FileStream(dst, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, 1, false);
                writeStream.SetLength(reportInfo.FileSize);
                (MemoryEx<byte>, int) item = default;
                while (await writeChannel.Reader.WaitToReadAsync(linkedCancelToken).ConfigureAwait(false))
                {
                    linkedCancelToken.ThrowIfCancellationRequested();
                    while (writeChannel.Reader.TryRead(out item))
                    {
                        try
                        {
                            var data = item.Item1.Data;
                            var readSize = item.Item2;
                            writeStream.Write(data.Span.Slice(0, readSize));
                            reportInfo.AddWritedSize(readSize);
                        }
                        finally
                        {
                            memoryPool.Return(item.Item1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Delete WritingFile
                if (System.IO.File.Exists(dst))
                {
                    System.IO.File.Delete(dst);
                }
                linkedCancelTokenSource.Cancel();

                // Read Failed
                if (ex is OperationCanceledException canceledException &&
                    canceledException.CancellationToken == linkedCancelToken)
                {
                    return;
                }
                throw;
            }
        }, linkedCancelToken);

        // ReadTask
        var readTask = Task.Run(() =>
        {
            try
            {
                using var readStream = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.Read, 1, false);
                while (readStream.Position != readStream.Length)
                {
                    linkedCancelToken.ThrowIfCancellationRequested();
                    var memoryData = memoryPool.Rent();
                    var readSize = readStream.Read(memoryData.Data.Span);
                    reportInfo.AddReadedSize(readSize);
                    if (!writeChannel.Writer.TryWrite((memoryData,readSize)))
                    {
                        throw new InvalidOperationException($"Write ReadData Error.");
                    }
                }
                writeChannel.Writer.Complete();
            }
            catch (Exception ex)
            {
                linkedCancelTokenSource.Cancel();

                // Write Failed
                if (ex is OperationCanceledException canceledException &&
                    canceledException.CancellationToken == linkedCancelToken)
                {
                    return;
                }
                throw;
            }

        }, linkedCancelToken);


        // Wait Tasks
        try
        {
            await Task.WhenAll(readTask, writeTask).ConfigureAwait(false);
        }
        finally
        {
            if (!linkedCancelTokenSource.IsCancellationRequested)
            {
                linkedCancelTokenSource.Cancel();
            }
            linkedCancelTokenSource.Dispose();
        }

        // FileInfo
        var srcFileInfo = new System.IO.FileInfo(src);
        var dstFileInfo = new System.IO.FileInfo(dst);
        dstFileInfo.Attributes |= srcFileInfo.Attributes & option.CopyAttributes;
        if (option.CopyDates.HasFlag(FileDates.Create))
        {
            dstFileInfo.CreationTimeUtc = srcFileInfo.CreationTimeUtc;
        }
        if (option.CopyDates.HasFlag(FileDates.LastAccess))
        {
            dstFileInfo.LastAccessTimeUtc = srcFileInfo.LastAccessTimeUtc;
        }
        if (option.CopyDates.HasFlag(FileDates.LastWrite))
        {
            dstFileInfo.LastWriteTimeUtc = srcFileInfo.LastWriteTimeUtc;
        }

        // Report Wait
        try
        {
            await progressTask.ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            // Through
        }
    }
}
