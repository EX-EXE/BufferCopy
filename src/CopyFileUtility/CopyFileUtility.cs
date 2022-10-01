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
            throw new System.IO.FileNotFoundException();
        }
        if (System.IO.File.Exists(dst))
        {
            if (!option.OverrideExistFile)
            {
                throw new System.IO.FileNotFoundException();
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

        // Memory
        var bufferSize = srcFileSize < option.BufferSize ? srcFileSize : option.BufferSize;
        var capacityCalc = Math.Ceiling((double)srcFileSize / (double)bufferSize);
        var capacitySize = capacityCalc < option.PoolCapacity ? capacityCalc : option.PoolCapacity;
        var memoryPool = new ThreadMemoryPool((int)bufferSize, (int)capacitySize);

        // Channel
        var channelOption = new BoundedChannelOptions(option.PoolCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true,
        };
        var writeChannel = Channel.CreateBounded<(ThreadMemory, int)>(channelOption);

        // WriteTask
        var writeTask = Task.Run(async () =>
        {
            try
            {
                using var writeStream = new FileStream(dst, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                // Write
                while (await writeChannel.Reader.WaitToReadAsync(linkedCancelToken))
                {
                    await foreach (var (memoryOwner, readSize) in writeChannel.Reader.ReadAllAsync(linkedCancelToken))
                    {
                        try
                        {
                            var writeSize = await WriteAsync(writeStream, memoryOwner.Memory.Slice(0, readSize), option, linkedCancelToken).ConfigureAwait(false);
                            reportInfo.AddWritedSize(writeSize);
                        }
                        finally
                        {
                            memoryOwner.Return();
                        }
                    }
                }
            }
            catch
            {
                linkedCancelTokenSource.Cancel();
                // Return AllItem
                while (await writeChannel.Reader.WaitToReadAsync())
                {
                    await foreach (var (memoryOwner, readSize) in writeChannel.Reader.ReadAllAsync())
                    {
                        memoryOwner.Return();
                    }
                }
                throw;
            }
            finally
            {
                if (linkedCancelToken.IsCancellationRequested && System.IO.File.Exists(dst))
                {
                    System.IO.File.Delete(dst);
                }
            }
        }, linkedCancelToken);

        // Read
        try
        {
            using var readStream = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.Read);
            while (readStream.Position != readStream.Length)
            {
                var memoryData = await memoryPool.RentAsync(linkedCancelToken).ConfigureAwait(false);
                var readSize = await ReadAsync(readStream, memoryData.Memory, option, linkedCancelToken).ConfigureAwait(false);
                reportInfo.AddReadedSize(readSize);
                await writeChannel.Writer.WriteAsync((memoryData, readSize), linkedCancelToken).ConfigureAwait(false);
            }
        }
        finally
        {
            writeChannel.Writer.Complete();
        }

        // Write Wait
        await writeTask.ConfigureAwait(false);
        linkedCancelToken.ThrowIfCancellationRequested();

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