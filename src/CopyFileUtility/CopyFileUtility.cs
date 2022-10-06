using CopyFileUtility_Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

public partial class CopyFileUtility
{
    public static async ValueTask CopyAsync(
        string src,
        string dst,
        CopyFileOptions option,
        IProgress<CopyFileProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // Check
        cancellationToken.ThrowIfCancellationRequested();
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
            System.IO.File.Delete(dst);
        }
        // File
        var srcFileSize = new System.IO.FileInfo(src.ToString()).Length;

        // Token
        using var linkedCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var linkedCancelToken = linkedCancelTokenSource.Token;

        // Report
        var reportInfo = new CopyFileProgress()
        {
            StartDate = DateTime.Now,
            FileSize = srcFileSize
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
                    // Through
                }
            }
        }, linkedCancelToken);

        try
        {
            // Memory
            var bufferSize = srcFileSize < option.BufferSize ? srcFileSize : option.BufferSize;
            var memoryPool = new ThreadMemoryPool((int)bufferSize);

            // Channel
            var channelOption = new BoundedChannelOptions(ThreadMemoryPool.PoolSize)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true,
            };
            var writeChannel = Channel.CreateBounded<(Memory<byte>, int)>(channelOption);

            // WriteTask
            var writeTask = Task.Run(async () =>
            {
                using var writeStream = new FileStream(dst, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                writeStream.SetLength(srcFileSize);
                // Write
                while (await writeChannel.Reader.WaitToReadAsync(linkedCancelToken).ConfigureAwait(false))
                {
                    await foreach (var (memoryData, bitNum) in writeChannel.Reader.ReadAllAsync(linkedCancelToken).ConfigureAwait(false))
                    {
                        linkedCancelToken.ThrowIfCancellationRequested();
                        try
                        {
                            var writeSize = await WriteAsync(writeStream, memoryData, option, default).ConfigureAwait(false);
                            reportInfo.AddWritedSize(writeSize);
                        }
                        finally
                        {
                            memoryPool.Return(bitNum);
                        }
                    }
                }
            }, linkedCancelToken);

            // Read
            using var readStream = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.Read);
            while (readStream.Position != readStream.Length)
            {
                var (memoryData, bitNum) = memoryPool.Rent();
                var readSize = await ReadAsync(readStream, memoryData, option, linkedCancelToken).ConfigureAwait(false);
                reportInfo.AddReadedSize(readSize);
                await writeChannel.Writer.WriteAsync((memoryData.Slice(0, readSize), bitNum), linkedCancelToken).ConfigureAwait(false);
            }
            writeChannel.Writer.Complete();

            // WriteWait
            await writeTask.ConfigureAwait(false);
            linkedCancelToken.ThrowIfCancellationRequested();
        }
        finally
        {
            if(linkedCancelToken.IsCancellationRequested)
            {
                if (System.IO.File.Exists(dst))
                {
                    System.IO.File.Delete(dst);
                }
            }
        }

        // Report Wait
        linkedCancelTokenSource.Cancel();
        await progressTask.ConfigureAwait(false);
    }

    private static async ValueTask<int> WriteAsync(
        Stream stream,
        Memory<byte> data,
        CopyFileOptions option,
        CancellationToken cancellationToken)
    {
        var currentRetryCount = 0;
        while (true)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                return data.Length;
            }
            catch (Exception ex)
            {
                // Canceled
                if (ex is OperationCanceledException)
                {
                    throw;
                }

                if (currentRetryCount < option.RetryCount)
                {
                    // Retry
                    ++currentRetryCount;
                    await Task.Delay(option.RetryInterval, cancellationToken).ConfigureAwait(false);
                    continue;
                }
                else
                {
                    // Rethrow
                    throw;
                }
            }
        }
    }

    private static async ValueTask<int> ReadAsync(
        Stream stream,
        Memory<byte> data,
        CopyFileOptions option,
        CancellationToken cancellationToken)
    {
        var currentRetryCount = 0;
        var startPosition = stream.Position;
        while (true)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await stream.ReadAsync(data, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Canceled
                if (ex is OperationCanceledException)
                {
                    throw;
                }

                if (currentRetryCount < option.RetryCount)
                {
                    // Retry
                    ++currentRetryCount;
                    await Task.Delay(option.RetryInterval, cancellationToken).ConfigureAwait(false);
                    if (stream.Position != startPosition)
                    {
                        stream.Position = startPosition;
                    }
                    continue;
                }
                else
                {
                    // Rethrow
                    throw;
                }
            }
        }
    }
}